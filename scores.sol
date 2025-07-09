// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts-upgradeable/access/OwnableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/security/PausableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/Initializable.sol";

contract Scores is Initializable, OwnableUpgradeable, PausableUpgradeable {
    struct Score {
        uint256 score;
        uint256 timestamp;
        mapping(string => uint256) additionalData;
    }

    // --- State ---
    mapping(address => Score) private scores;
    mapping(address => bool) public admins;

    // --- Events ---
    event ScoreUpdated(
        address indexed wallet,
        uint256 score,
        uint256 timestamp,
        string[] keys,
        uint256[] values
    );
    event AdminAdded(address indexed admin);
    event AdminRemoved(address indexed admin);

    // --- Initializer ---
    function initialize(address initialOwner) public initializer {
        __Ownable_init(initialOwner);
        __Pausable_init();
    }

    // --- Admin Management ---
    function addAdmin(address _admin) external onlyOwner {
        require(_admin != address(0), "Invalid address");
        admins[_admin] = true;
        emit AdminAdded(_admin);
    }

    function removeAdmin(address _admin) external onlyOwner {
        require(admins[_admin], "Address is not an admin");
        admins[_admin] = false;
        emit AdminRemoved(_admin);
    }

    // --- Core: setScores with optional metadata ---
    function setScores(
        address[] calldata wallets,
        uint256[] calldata scores_,
        string[][] calldata keys,
        uint256[][] calldata values
    ) external whenNotPaused {
        require(msg.sender == owner() || admins[msg.sender], "Not authorized");
        require(wallets.length == scores_.length, "Wallets and scores mismatch");

        // If either keys or values provided, both must match wallets.length
        bool hasMetadata = keys.length > 0 || values.length > 0;
        if (hasMetadata) {
            require(keys.length == wallets.length, "Wallets and keys mismatch");
            require(values.length == wallets.length, "Wallets and values mismatch");
        }

        for (uint256 i = 0; i < wallets.length; i++) {
            address wallet = wallets[i];
            require(wallet != address(0), "Invalid wallet");

            Score storage s = scores[wallet];
            s.score = scores_[i];
            s.timestamp = block.timestamp;

            // Prepare event arrays
            string[] memory keysToEmit;
            uint256[] memory valsToEmit;

            if (hasMetadata && keys[i].length > 0) {
                require(keys[i].length == values[i].length, "Key/value len mismatch");

                // Store on-chain
                for (uint256 j = 0; j < keys[i].length; j++) {
                    s.additionalData[keys[i][j]] = values[i][j];
                }

                // Use provided metadata for event
                keysToEmit = keys[i];
                valsToEmit = values[i];
            } else {
                // Zero-length arrays for event
                keysToEmit = new string[](0); // Corrected
                valsToEmit = new uint256[](0); // Corrected
            }

            emit ScoreUpdated(wallet, s.score, s.timestamp, keysToEmit, valsToEmit);
        }
    }

    // --- View Helpers ---
    /// @notice Read one piece of additionalData
    function getAdditionalData(address wallet, string calldata key)
        external
        view
        returns (uint256)
    {
        return scores[wallet].additionalData[key];
    }

    /// @notice Retrieve score & timestamp
    function getScore(address wallet)
        external
        view
        returns (uint256 score, uint256 timestamp)
    {
        Score storage s = scores[wallet];
        return (s.score, s.timestamp);
    }

    // --- Pause Control ---
    function pause() external onlyOwner {
        _pause();
    }
    function unpause() external onlyOwner {
        _unpause();
    }
}
