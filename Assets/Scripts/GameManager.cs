using UnityEngine;

/**
 * GameManager that links TetrisGameManager to the key controls.
 * This is a very limited small class just for demo purposes
 * and not all controls have been implemented.
 */
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
	[SerializeField] private TetrisGameManager tetrisGameManager;

	private void Update ()
	{
		//Simple hard coded key controls, but they are external of the TetrisGameManager and easily updated
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			tetrisGameManager.RotatePieceRight();
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			tetrisGameManager.RotatePieceLeft();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) tetrisGameManager.MovePieceLeft();
		if (Input.GetKeyDown(KeyCode.RightArrow)) tetrisGameManager.MovePieceRight();
		if (Input.GetKeyDown(KeyCode.DownArrow)) tetrisGameManager.MovePieceDown();
		if (Input.GetKeyDown(KeyCode.Space)) tetrisGameManager.DropPiece();
		if (Input.GetKeyDown(KeyCode.H)) tetrisGameManager.HoldPiece();

	}

	public void StartGame()
	{
		tetrisGameManager.StartGame();
	}
}

