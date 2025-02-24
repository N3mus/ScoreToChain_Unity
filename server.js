import express from "express";
import { ethers, JsonRpcProvider, Wallet } from "ethers";
import dotenv from "dotenv";
import scores from "./scores.json" assert { type: "json" };

dotenv.config();

const app = express();
const port = 8000; // Studio's backend API runs on this port
const provider = new JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY; // Private key for signing transactions
const wallet = new Wallet(privateKey, provider);
const contractAddress = "0xYourMatchScoresContractAddressHere"; // Studio's contract address

const matchScoresContract = new ethers.Contract(contractAddress, scores.abi, wallet);

// Middleware to parse JSON
app.use(express.json());

app.post("/postMatchResults", async (req, res) => {
    console.log("Received request from Unity:", req.body);

    const { address, uuid, amount } = req.body;

    if (!address || !uuid || !amount) {
        return res.status(400).send({ error: "Missing parameters" });
    }

    try {
        // Convert amount to wei
        const amountInWei = ethers.parseEther(amount.toString());

        // Send the transaction directly to the blockchain
        const tx = await matchScoresContract.setScore(address, uuid, amountInWei);
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
