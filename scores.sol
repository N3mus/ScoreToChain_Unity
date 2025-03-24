// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "@openzeppelin/contracts-upgradeable/access/OwnableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/security/PausableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/Initializable.sol";

contract Scores is Initializable, OwnableUpgradeable, PausableUpgradeable {
    struct Score {
        int256 score;
        uint256 timestamp;
    }

    mapping(address => Score) public scores;
    mapping(address => mapping(string => uint256)) public additionalMatchData;
    mapping(address => bool) public admins;

    event ScoreUpdated(
        address indexed wallet,
        int256 score,
        uint256 timestamp,
        string[] keys,
        uint256[] values
    );

    event ScoreKeyValue(
        address indexed wallet,
        string key,
        uint256 value
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

    /// @notice Allows owner and admins to update scores (including negative values)
    function setScore(
        address _wallet,
        int256 _score,
        string[] memory keys,
        uint256[] memory values
    ) external whenNotPaused {
        require(_wallet != address(0), "Invalid wallet address");
        require(msg.sender == owner() || admins[msg.sender], "Not authorized");
        require(keys.length == values.length, "Mismatched key-value array length");

        // Create or update score entry
        Score storage userScore = scores[_wallet];
        userScore.score = _score;
        userScore.timestamp = block.timestamp;

        // Store additional match data and emit individual events
        for (uint256 i = 0; i < keys.length; i++) {
            additionalMatchData[_wallet][keys[i]] = values[i];
            emit ScoreKeyValue(_wallet, keys[i], values[i]); // 🔥 easier to decode on Etherscan
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
