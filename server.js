const express = require("express");
const { ethers } = require("ethers");
const bs58 = require("bs58");
const { decodeAddress } = require("@polkadot/util-crypto");
const dotenv = require("dotenv");
const ScoresJSON = require("./Scores.json");

dotenv.config();

const app = express();
const port            = process.env.PORT || 8000;
const provider        = new ethers.JsonRpcProvider(process.env.ETH_NODE_URL);
const privateKey      = process.env.STUDIO_PRIVATE_KEY;
const contractAddress = process.env.MATCH_SCORES_CONTRACT;

if (!privateKey || !contractAddress || !process.env.ETH_NODE_URL) {
  console.error("❌ Missing env vars");
  process.exit(1);
}

const wallet = new ethers.Wallet(privateKey, provider);
const scoresContract = new ethers.Contract(contractAddress, ScoresJSON.abi, wallet);
app.use(express.json());

/**
 * Normalize any 32-byte pubkey into a 0x-prefixed 32-byte hex string.
 * Supports:
 *   • Ethereum (0x…)  
 *   • Solana (Base58, 32 bytes)  
 *   • Substrate/SS58 (Base58, up to 32 bytes decoded)
 */
function toBytes32Key(raw) {
  // 1) Ethereum?
  if (ethers.isAddress(raw)) {
    // Zero-pad 20-byte address → 32 bytes
    return ethers.hexZeroPad(raw, 32);
  }

  // 2) Solana? (32-byte Base58)
  try {
    const decoded = bs58.decode(raw);
    if (decoded.length === 32) {
      return "0x" + Buffer.from(decoded).toString("hex");
    }
  } catch (_) {
    // not valid base58 or wrong length; fallthrough
  }

  // 3) Substrate/SS58?
  try {
    const pubkey = decodeAddress(raw);   // returns Uint8Array of length 32
    return "0x" + Buffer.from(pubkey).toString("hex");
  } catch (_) {
    // not SS58
  }

  throw new Error("Unsupported wallet format");
}

app.post("/postMatchResults", async (req, res) => {
  console.log("→", req.body);
  const { address, amount, keys, values } = req.body;

  if (!address || amount == null || !keys || !values) {
    return res.status(400).send({ error: "Missing parameters" });
  }
  if (keys.length !== values.length) {
    return res.status(400).send({ error: "Keys and values length mismatch" });
  }

  let walletKey;
  try {
    walletKey = toBytes32Key(address);
  } catch (err) {
    return res.status(400).send({ error: err.message });
  }

  try {
    const scoreWei = ethers.parseEther(amount.toString());
    console.log(`➡️ setScore(${walletKey}, ${scoreWei.toString()}, …)`);

    const tx = await scoresContract.setScore(
      walletKey,
      scoreWei,
      keys,
      values
    );
    const receipt = await tx.wait();

    if (receipt.status === 1) {
      console.log("✅", tx.hash);
      return res.send({ message: "success", txHash: tx.hash });
    } else {
      console.log("❌ receipt failed", receipt);
      return res.send({ message: "fail" });
    }
  } catch (error) {
    console.error("⚠️ setScore error:", error);
    return res.status(500).send({ error: "Failed", details: error.message });
  }
});

app.listen(port, () => {
  console.log(`🚀 API running on http://localhost:${port}`);
});
