// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "@openzeppelin/contracts-upgradeable/access/OwnableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/security/PausableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/Initializable.sol";

contract Scores is Initializable, OwnableUpgradeable, PausableUpgradeable {
    struct Score {
        uint256 score;
        uint256 timestamp;
        mapping(string => uint256) additionalData; // Flexible additional match data
    }

    mapping(address => Score) public scores;
    mapping(address => bool) public admins; // List of allowed studios (admins)

    event ScoreUpdated(
        address indexed wallet,
        uint256 score,
        uint256 timestamp,
        string[] keys,
        uint256[] values
    );
    
    event AdminAdded(address indexed admin);
    event AdminRemoved(address indexed admin);

    function initialize(address initialOwner) public initializer {
        __Ownable_init(initialOwner);
        __Pausable_init();
    }

    /// @notice Allows owner to add an admin (studio) that can update scores
    function addAdmin(address _admin) external onlyOwner {
        require(_admin != address(0), "Invalid address");
        admins[_admin] = true;
        emit AdminAdded(_admin);
    }

    /// @notice Allows owner to remove an admin
    function removeAdmin(address _admin) external onlyOwner {
        require(admins[_admin], "Address is not an admin");
        admins[_admin] = false;
        emit AdminRemoved(_admin);
    }

    /// @notice Allows owner and admins to update scores
    function setScore(
        address _wallet,
        uint256 _score,
        string[] memory keys,
        uint256[] memory values
    ) external whenNotPaused {
        require(_wallet != address(0), "Invalid wallet address");
        require(msg.sender == owner() || admins[msg.sender], "Not authorized");

        // Create or update score entry
        Score storage userScore = scores[_wallet];
        userScore.score = _score;
        userScore.timestamp = block.timestamp;

        // Store additional match data dynamically
        require(keys.length == values.length, "Mismatched key-value array length");
        for (uint256 i = 0; i < keys.length; i++) {
            userScore.additionalData[keys[i]] = values[i];
        }

        emit ScoreUpdated(_wallet, _score, block.timestamp, keys, values);
    }

    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }
}
