using UnityEngine;

/**
 * GameManager that links BloxGameManager to the key controls.
 * This is a very limited small class just for demo purposes
 * and not all controls have been implemented.
 */
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
	[SerializeField] private BloxGameManager bloxGameManager;

	private void Update ()
	{
		//Simple hard coded key controls, but they are external of the BloxGameManager and easily updated
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			bloxGameManager.RotatePieceRight();
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			bloxGameManager.RotatePieceLeft();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) bloxGameManager.MovePieceLeft();
		if (Input.GetKeyDown(KeyCode.RightArrow)) bloxGameManager.MovePieceRight();
		if (Input.GetKeyDown(KeyCode.DownArrow)) bloxGameManager.MovePieceDown();
		if (Input.GetKeyDown(KeyCode.Space)) bloxGameManager.DropPiece();
		if (Input.GetKeyDown(KeyCode.H)) bloxGameManager.HoldPiece();

	}

	public void StartGame()
	{
		bloxGameManager.StartGame();
	}
}

