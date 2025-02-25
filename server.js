import express from "express";
import { ethers, JsonRpcProvider, Wallet } from "ethers";
import dotenv from "dotenv";
import ScoresJSON from "./Scores.json" assert { type: "json" };

dotenv.config();

const app = express();
const port = 8000;
const provider = new JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY;
const wallet = new Wallet(privateKey, provider);
const contractAddress = "0xYourMatchScoresContractAddressHere";

const scoresContract = new ethers.Contract(contractAddress, ScoresJSON.abi, wallet);

// Middleware
app.use(express.json());

app.post("/postMatchResults", async (req, res) => {
    console.log("Received request:", req.body);

    const { address, uuid, amount, keys, values } = req.body;

    if (!address || !uuid || !amount || !keys || !values) {
        return res.status(400).json({ error: "Missing parameters" });
    }
    if (keys.length !== values.length) {
        return res.status(400).json({ error: "Keys and values arrays must be the same length" });
    }

    try {
        const amountInWei = ethers.parseEther(amount.toString());

        console.log(`Sending transaction: Player=${address}, Score=${amountInWei.toString()}, Data=${keys}`);

        const tx = await scoresContract.setScore(address, amountInWei, keys, values);
        const receipt = await tx.wait();

        if (receipt.status === 1) {
            console.log(`✅ Transaction Successful! TX Hash: ${tx.hash}`);
            res.json({ message: "success", txHash: tx.hash });
        } else {
            console.log("❌ Transaction failed!", receipt);
            res.json({ message: "fail" });
        }
    } catch (error) {
        console.error("Error submitting score:", error);
        res.status(500).json({ error: "Failed to submit score", details: error.message });
    }
});

// Start server
app.listen(port, () => {
    console.log(`Studio API is running on http://localhost:${port}`);
});
