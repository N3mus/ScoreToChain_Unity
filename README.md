```markdown
# Studio Backend API & Game Integration for Blockchain Score Submission

## Overview
This repository provides code and documentation for integrating blockchain-based score submissions in your game. It includes:

- **Backend API (`server.js`)**: A Node.js Express server that batches and submits transactions to the blockchain via the `Scores` smart contract.
- **Unity Integration (`ScoreToApi.cs`)**: A Unity C# script that logs, queues, and batches player scores (and metadata) to the backend API.
- **Unreal Integration (`ScoreManagerUnreal.h/.cpp`)**: An Unreal C++ module/component that mirrors the Unity batching, logging, and HTTP submission.
- **Smart Contract Reference (`scores.sol`)**: The Solidity contract defining how scores and metadata are stored on-chain.

---

## Repository Structure
```

/ (root)
├─ server.js               # Node.js backend API
├─ Scores.json             # ABI for the Scores smart contract
├─ scores.sol              # Solidity source (for reference)
├─ ScoreToApi.cs           # Unity batching & upload component
├─ Unreal                  # Unreal header & source for batching & upload
├─ README.md               # This file
└─ .env                    # Environment variables (example in repo)

````

---

## Prerequisites
- **Node.js** (v16+)
- **Unity** (with JSON.NET / Newtonsoft.Json)
- **Unreal Engine** (with HTTP & JSON modules)
- Deployed **Scores** smart contract on an Ethereum-compatible chain
- A wallet with funds for gas fees (testnet recommended)
- `.env` file configured (see below)

---

## Environment Setup
Create a `.env` file in the project root:

```ini
# Blockchain RPC URL
ETH_NODE_URL=https://rpc.api.moonbeam.network

# Studio's Private Key (test key for development)
STUDIO_PRIVATE_KEY=0xYourPrivateKeyHere

# Deployed contract address
MATCH_SCORES_CONTRACT=0xYourMatchScoresContractAddressHere

# Backend port (optional)
PORT=8000
````

> **Security:** Never commit real private keys. Use environment variables.

---

## Installation

1. **Clone**

   ```bash
   git clone https://github.com/your-repo/studio-backend.git
   cd studio-backend
   ```
2. **Install dependencies**

   ```bash
   npm install
   ```
3. **Place ABI**
   Ensure `Scores.json` is in the root directory.

---

## Backend: `server.js`

The Express server exposes:

* **Endpoint:** `POST /postMatchResults` accepts a **batch** payload:

  ```json
  {
    "wallets": ["0x..", ...],
    "scores":  [123, 456, ...],
    "keys":    [["k1","k2"], ...],
    "values":  [[10,20], ...]
  }
  ```
* **Validation:** checks array lengths, converts scores to BigInt.
* **Contract call:**

  ```js
  await scoresContract.setScores(
    wallets,
    scoreBigInts,
    keysArg,
    valsArg
  );
  ```

---

## Unity: `ScoreToApi.cs`

* **ScoreEntry** model with `wallet`, `score`, and `meta`.
* **ScoreQueue**: in-memory queue + persistent `scores.log` under `Application.persistentDataPath`.
* **ScoreUploader**: sends batched JSON to the backend.
* **ScoreManager**: enqueues entries, flushes when batch size (default 50) is reached, every minute, and on application quit.

Attach `ScoreManager` to a GameObject and call:

```csharp
ScoreManager.Instance.RecordScore(
    "0x123…", 
    100, 
    new Dictionary<string,int>{{"kills",5}}
);
```

---

## Unreal: `ScoreManagerUnreal` (h/.cpp)

* **FScoreEntry** USTRUCT for data.
* **UScoreQueueComponent**: in-memory queue + saves `Saved/scores.log`.
* **AScoreManager** Actor:

  * `RecordScore(...)` to enqueue.
  * Timed and size-based `FlushNow()` to build JSON and POST to `/postMatchResults`.
  * Logs successes/failures in the output log.

Place the files in your `Source/` folder, add the module to your build, and spawn or place `AScoreManager` in your level.

---

## Smart Contract Reference: `scores.sol`

Defines the `Scores` contract with:

* `setScores(address[] wallets, uint256[] scores_, string[][] keys, uint256[][] values)`
* Batch storage & events: `ScoreUpdated(address,uint256,uint256,string[],uint256[])`

Use this file to verify against your deployed address.

---

## Final Notes

* Update API URLs in Unity/Unreal to your hosted backend.
* Adjust `BATCH_SIZE` and `FlushInterval` as needed for your game’s throughput.
* Ensure `.env` is secure and never committed.

---

## Support

Open an issue or contact the maintainers for any questions.

Happy coding! 🚀

```
```
