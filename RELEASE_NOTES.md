# 📦 Release Notes – `Scores v2` (March 2025)

## 🔄 Summary
This release introduces support for **negative scores**, **improved event logging for Etherscan visibility**, and **updated backend & Unity compatibility**.

---

## 🛠 Smart Contract Adjustments (`Scores.sol`)

### ✅ Changes
- **Score Type Changed to `int256`:**  
  - Allows for **negative scores** in `setScore()` and `scores()` return value.

- **Mapping Restructure:**
  - Removed mapping from inside the struct for better ABI compatibility.
  - New mapping:
    ```solidity
    mapping(address => mapping(string => uint256)) public additionalMatchData;
    ```

- **Event Improvements:**
  - New event `ScoreKeyValue(address indexed wallet, string key, uint256 value)` added to emit match data individually for better Etherscan decoding.

- **Updated ABI (`Scores.json`):**
  - Updated to reflect the `int256` changes.
  - Includes `ScoreKeyValue` event for improved frontend/backend handling.

---

## 🔧 Backend Server Adjustments (`server.js`)

### ✅ Changes
- **Support for `int256`:**
  - Removed `ethers.parseEther()` (was used for ETH parsing).
  - Now uses:
    ```js
    const parsedScore = ethers.toBigInt(amount);
    ```
  - Accepts both positive and negative scores properly.

- **Additional Improvements:**
  - Values are converted to `BigInt`.
  - Improved input validation and logging.

---

## 🎮 Unity Client Changes (`ScoreUploader.cs`)

### ✅ Action Items for Unity Devs

1. **Send Score as a String:**
   ```csharp
   { "amount", score.ToString() } // can now be negative (e.g., "-50")
   ```

2. **Ensure Correct Payload Format:**
   - `keys`: List of strings → `List<string>`
   - `values`: List of integers → `List<int>`

3. **Example Payload Sent from Unity:**
   ```json
   {
     "address": "0xUserWallet",
     "amount": "-42",
     "keys": ["kills", "damage"],
     "values": [5, 1200]
   }
   ```

4. **No Changes Required to Endpoint or Headers.**

---

## ✅ Final Checklist

| Component         | Status   | Description                             |
|------------------|----------|-----------------------------------------|
| Smart Contract    | ✅ Done  | Supports negative scores & logs key/value pairs |
| Backend (`server.js`) | ✅ Done  | Now handles `int256` correctly and logs inputs |
| Unity Client      | ✅ Done  | Just ensure score is sent as a **string**, including negatives |

---

## 🧪 Testing

Please test on testnet with:
- ✅ A negative score (e.g., `-25`)
- ✅ Multiple match keys and values (e.g., `"kills"`, `"assists"`, `"damage"`)

---

## 📬 Questions?
Ping Neal or drop a message in our telegram channel
