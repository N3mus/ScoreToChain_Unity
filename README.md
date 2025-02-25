# Studio Backend API & Unity Integration for Blockchain Score Submission

## Overview
This repository provides the necessary code and documentation for integrating blockchain-based score submissions in your game. It is designed for studios to host their own backend API and integrate with Unity to send player scores and match metadata directly to the blockchain.

**Key Components:**
- **Backend API (`server.js`)**: A Node.js Express server that signs and submits transactions to the blockchain using the provided smart contract.
- **Unity Integration (`ScoreToApi.cs`)**: A Unity C# script that sends player scores and additional match data from the game client to the studio's backend API.
- **Smart Contract Reference (`scores.sol`)**: The Solidity smart contract that defines how scores are stored and updated. *Note: We will provide the smart contract address separately; this file is for reference.*

---

## Repository Structure
- `server.js` – The backend API that receives score submissions, signs transactions, and submits them to the blockchain.
- `ScoreToApi.cs` – The Unity C# script used to send player scores and match data to the backend API.
- `Scores.json` – The ABI file for the Scores smart contract.
- `scores.sol` – The Solidity smart contract (provided for reference).
- `.env` – Environment file (not included in the repository) that contains sensitive configuration variables.

---

## Prerequisites
- **Node.js** (v16+ recommended) and NPM/Yarn.
- **Unity** with JSON.NET (Newtonsoft.Json) installed (via Unity Package Manager or as a DLL).
- A deployed **Scores** smart contract on an Ethereum-compatible blockchain (e.g., Moonbeam).
- A wallet with sufficient funds for gas fees (for testing, use a test wallet).
- A properly configured `.env` file (see below).

---

## Environment Setup
Create a `.env` file in the root directory with the following variables:

```ini
# Blockchain RPC URL (e.g., Moonbeam)
ETH_NODE_URL=https://rpc.api.moonbeam.network

# Studio's Private Key (use a test key for development)
STUDIO_PRIVATE_KEY=0xYourPrivateKeyHere

# Contract address for the Match Scores contract (provided separately)
MATCH_SCORES_CONTRACT=0xYourMatchScoresContractAddressHere

# Server Port (Optional: Change if running multiple services)
PORT=8000
```

> **Security Note:** Never expose your real private key in public repositories. Use environment variables to keep sensitive data secure.

---

## Installation

### 1. Clone the Repository
```bash
git clone https://github.com/your-repo/studio-backend.git
cd studio-backend
```

### 2. Install Dependencies
```bash
npm install
```

### 3. Place the ABI File
Ensure that `Scores.json` (the ABI file for the Scores contract) is placed in the root directory.

---

## How `server.js` Works
The `server.js` file implements an Express server that listens on a specified port (default is `8000`) and exposes a POST endpoint `/postMatchResults`. It performs the following tasks:

- **Environment Variables:**  
  Loads the blockchain RPC URL, studio's private key, contract address, and server port from the `.env` file.

- **Blockchain Connection:**  
  Connects to the blockchain using the `ethers` library and creates a wallet instance using the private key.

- **Smart Contract Interaction:**  
  Connects to the Scores smart contract using its ABI (from `Scores.json`) and the contract address provided via the `.env` file.

- **API Endpoint (`/postMatchResults`)**:  
  Expects a JSON payload with the following parameters:
  - `address` (string): The player's wallet address.
  - `amount` (string): The score to record (sent as a string and converted to Wei).
  - `keys` (array): An array of strings representing additional match data keys.
  - `values` (array): An array of integers corresponding to each key.
  
  It validates the payload, converts the score to Wei, calls the smart contract's `setScore` function, waits for the transaction to be mined, and returns the transaction hash upon success.

---

## How `ScoreToApi.cs` Works
The `ScoreToApi.cs` file is a Unity C# script that sends player score data along with additional match metadata to the studio's backend API.

**Functionality:**
- **Data Preparation:**  
  Accepts a player's wallet address, a score (as a float), and a dictionary containing additional match data (e.g., kills, assists, time played). It constructs a dictionary that includes:
  - `address`: The player's wallet address.
  - `amount`: The score converted to a string (for API compatibility).
  - `keys`: An array of strings (extracted from the additional data keys).
  - `values`: An array of integers (extracted from the additional data values).

- **JSON Serialization:**  
  Uses JSON.NET (Newtonsoft.Json) to properly serialize the data into JSON format.

- **POST Request:**  
  Sends a POST request to the backend API endpoint (`/postMatchResults`) with the JSON payload. Logs the server response to help with debugging.

---

## Smart Contract Reference
The `scores.sol` file is provided as a reference and defines the structure and functionality for storing player scores and additional match data on the blockchain. The key function is `setScore`, which updates a player's score along with flexible metadata.

---

## Final Notes
- The studio's backend (`server.js`) expects the smart contract address to be provided via the `.env` file. We will provide the smart contract address separately.
- The `scores.sol` file is included as a reference and should match the deployed smart contract.
- Update the API URL in `ScoreToApi.cs` to match the actual endpoint where your studio backend is hosted.
- This setup allows for secure and scalable blockchain score submissions from Unity to the blockchain via your studio’s backend.

---

## Contact & Support
For questions or issues, please open a GitHub issue in this repository or contact the maintainers directly.

Happy coding and blockchain integration! 🚀
