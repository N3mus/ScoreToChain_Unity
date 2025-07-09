const express = require("express");
const { ethers } = require("ethers");
const dotenv = require("dotenv");
const ScoresJSON = require("./Scores.json");

dotenv.config();

const app = express();
const port = process.env.PORT || 8000;
const provider = new ethers.JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey = process.env.STUDIO_PRIVATE_KEY;
const contractAddress = process.env.MATCH_SCORES_CONTRACT;

if (!privateKey || !contractAddress || !process.env.ETH_NODE_URL) {
  console.error("❌ Missing environment variables. Please check your .env file.");
  process.exit(1);
}

const wallet = new ethers.Wallet(privateKey, provider);
const scoresContract = new ethers.Contract(contractAddress, ScoresJSON.abi, wallet);

app.use(express.json());

app.post("/postMatchResults", async (req, res) => {
  console.log("Received batch request from Unity:", req.body);

  const { wallets, scores, keys, values } = req.body;

  // Basic validation
  if (
    !Array.isArray(wallets) ||
    !Array.isArray(scores) ||
    wallets.length === 0 ||
    wallets.length !== scores.length
  ) {
    return res.status(400).send({
      error: "Must provide non-empty arrays 'wallets' and 'scores' of the same length",
    });
  }

  const hasMetadata = Array.isArray(keys) || Array.isArray(values);
  if (hasMetadata) {
    if (
      !Array.isArray(keys) ||
      !Array.isArray(values) ||
      keys.length !== wallets.length ||
      values.length !== wallets.length
    ) {
      return res.status(400).send({
        error:
          "'keys' and 'values' must be arrays of length === wallets.length",
      });
    }
    // And each sub-array must match
    for (let i = 0; i < wallets.length; i++) {
      if (
        !Array.isArray(keys[i]) ||
        !Array.isArray(values[i]) ||
        keys[i].length !== values[i].length
      ) {
        return res.status(400).send({
          error: `At index ${i}, 'keys[${i}]' and 'values[${i}]' must both be arrays of the same length`,
        });
      }
    }
  }

  try {
    // Convert raw JS numbers/strings into BigInt scores
    const scoreBigInts = scores.map((s, i) => {
      try {
        return ethers.toBigInt(s);
      } catch {
        throw new Error(`Invalid numeric score at index ${i}: ${s}`);
      }
    });

    console.log(
      `Calling setScores with ${wallets.length} entries…`
    );

    // If no metadata provided, pass empty 2D arrays
    const keysArg = hasMetadata ? keys : [];
    const valsArg = hasMetadata ? values : [];

    const tx = await scoresContract.setScores(
      wallets,
      scoreBigInts,
      keysArg,
      valsArg
    );
    const receipt = await tx.wait();

    if (receipt.status === 1) {
      console.log(`✅ Batch transaction succeeded. TX Hash: ${tx.hash}`);
      res.send({ message: "success", txHash: tx.hash });
    } else {
      console.log("❌ Batch transaction failed", receipt);
      res.send({ message: "fail" });
    }
  } catch (error) {
    console.error("⚠️ Error in batch submission:", error);
    res.status(500).send({ error: "Batch submission failed", details: error.message });
  }
});

app.listen(port, () => {
  console.log(`🚀 Studio's API is running on http://localhost:${port}`);
});
