// SPDX-License-Identifier: MIT
pragma solidity 0.8.27;

contract Scores is Ownable, Pausable {
    struct Score {
        string gameid;
        uint256 score;
        uint256 timestamp;
    }

    constructor(address initialOwner) Ownable(initialOwner) Pausable() {
        _transferOwnership(initialOwner);  // Setting custom owner
    }

    mapping(address => Score) public scores;

    event ScoreUpdated(address indexed wallet, string gameid, uint256 score, uint256 timestamp);

    function setScore(address _wallet, string memory _gameid, uint256 _score) external onlyOwner whenNotPaused {
        require(bytes(_gameid).length == 36, "Game ID must be a valid UUID");
        scores[_wallet] = Score(_gameid, _score, block.timestamp);
        emit ScoreUpdated(_wallet, _gameid, _score, block.timestamp);
    }

    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }
}
