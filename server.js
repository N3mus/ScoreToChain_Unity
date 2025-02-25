import express from "express";
import { ethers, JsonRpcProvider, Wallet } from "ethers";
import dotenv from "dotenv";
import ScoresJSON from "./Scores.json" assert { type: "json" };

dotenv.config();

const app = express();
const port = 8000; // Studio's backend API runs on this port
const provider = new JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY; // Private key for signing transactions
const wallet = new Wallet(privateKey, provider);
const contractAddress = "0xYourMatchScoresContractAddressHere"; // Studio's contract address

const scoresContract = new ethers.Contract(contractAddress, ScoresJSON.abi, wallet);

// Middleware to parse JSON
app.use(express.json());

app.post("/postMatchResults", async (req, res) => {
    console.log("Received request from Unity:", req.body);

    const { address, amount, keys, values } = req.body;

    if (!address || !amount || !keys || !values) {
        return res.status(400).send({ error: "Missing parameters" });
    }
    if (keys.length !== values.length) {
        return res.status(400).send({ error: "Keys and values arrays must be the same length" });
    }

    try {
        // Convert amount to wei
        const amountInWei = ethers.parseEther(amount.toString());

        console.log(`Sending transaction: Player=${address}, Score=${amountInWei.toString()}, Data=${keys}`);

        // Send the transaction directly to the blockchain
        const tx = await scoresContract.setScore(address, amountInWei, keys, values);
        const receipt = await tx.wait();

        if (receipt.status === 1) {
            console.log(`Transaction successful! Hash: ${tx.hash}`);
            res.send({ message: "success", txHash: tx.hash });
        } else {
            console.log("Transaction failed!", receipt);
            res.send({ message: "fail" });
        }
    } catch (error) {
        console.error("Error submitting score:", error);
        res.status(500).send({ error: "Failed to submit score", details: error.message });
    }
});

// Start server
app.listen(port, () => {
    console.log(`Studio's API is running on http://localhost:${port}`);
});
