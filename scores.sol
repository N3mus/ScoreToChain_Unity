// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "@openzeppelin/contracts-upgradeable/access/OwnableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/security/PausableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/Initializable.sol";

contract Scores is Initializable, OwnableUpgradeable, PausableUpgradeable {
    struct Score {
        uint256 score;
        uint256 timestamp;
        mapping(string => uint256) additionalData;
    }

    mapping(bytes32 => Score) public scores;
    mapping(address => bool)   public admins;

    event ScoreUpdated(
        bytes32 indexed wallet,
        uint256    score,
        uint256    timestamp,
        string[]   keys,
        uint256[]  values
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
        bytes32       _wallet,   
        uint256       _score,
        string[] memory keys,
        uint256[] memory values
    ) external whenNotPaused {
        require(_wallet != bytes32(0), "Invalid wallet key");
        require(msg.sender == owner() || admins[msg.sender], "Not authorized");
        require(keys.length == values.length, "Key/value length mismatch");

        Score storage s = scores[_wallet];
        s.score     = _score;
        s.timestamp = block.timestamp;
        for (uint i = 0; i < keys.length; i++) {
            s.additionalData[keys[i]] = values[i];
        }

        emit ScoreUpdated(_wallet, _score, block.timestamp, keys, values);
    }

    function pause()   public onlyOwner { _pause(); }
    function unpause() public onlyOwner { _unpause(); }

    function ethToBytes32(address _eth) public pure returns (bytes32) {
        return bytes32(uint256(uint160(_eth)));
    }
}
