const express = require("express");
const { ethers } = require("ethers");
const dotenv = require("dotenv");
const ScoresJSON = require("./Scores.json");

dotenv.config();

const app = express();
const port = process.env.PORT || 8000; // Load PORT from .env, default to 8000
const provider = new ethers.JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY; // Private key for signing transactions
const contractAddress = process.env.MATCH_SCORES_CONTRACT; // Load contract address from .env

if (!privateKey || !contractAddress || !process.env.ETH_NODE_URL) {
    console.error("❌ Missing environment variables. Please check your .env file.");
    process.exit(1);
}

const wallet = new ethers.Wallet(privateKey, provider);
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
            console.log(`✅ Transaction successful! TX Hash: ${tx.hash}`);
            res.send({ message: "success", txHash: tx.hash });
        } else {
            console.log("❌ Transaction failed!", receipt);
            res.send({ message: "fail" });
        }
    } catch (error) {
        console.error("⚠️ Error submitting score:", error);
        res.status(500).send({ error: "Failed to submit score", details: error.message });
    }
});

// Start server
app.listen(port, () => {
    console.log(`🚀 Studio's API is running on http://localhost:${port}`);
});
