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

    mapping(bytes32 => Score) private scores; // Internal key is hash of _wallet (as string)
    mapping(address => bool) public admins;

    event ScoreUpdated(
        string _wallet,
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

    function setScore(
        string memory _wallet,
        uint256 _score,
        string[] memory keys,
        uint256[] memory values
    ) external whenNotPaused {
        require(bytes(_wallet).length > 0, "Wallet ID cannot be empty");
        require(msg.sender == owner() || admins[msg.sender], "Not authorized");
        require(keys.length == values.length, "Mismatched key-value array length");

        bytes32 userKey = keccak256(abi.encodePacked(_wallet));

        Score storage userScore = scores[userKey];
        userScore.score = _score;
        userScore.timestamp = block.timestamp;

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
