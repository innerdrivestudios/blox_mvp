using TMPro;
using UnityEngine;

/**
 * This ScoreKeeper awards points for every row cleared, but extra points for additional rows cleared.
 * 
 * This is the algorithm:
 * 1 row cleared -> 1 * the points
 * 2 rows cleared -> (1+2) * the points
 * 3 rows cleared -> (1+2+3) * the points
 * 4 rows cleared -> (1+2+3+4) * the points
 * n rows cleared -> n * (n+1) / 2 points
 */
public class ScoreKeeper : MonoBehaviour
{
	[SerializeField] private TMP_Text scoreField;
	[SerializeField] private int scorePerRow;
	
	private int score = 0;

	public void ResetScore()
	{
		score = 0;
	}

	public void RowsCleared (int pRowAmountCleared)
	{
		if (pRowAmountCleared < 1) return;
		score += scorePerRow * pRowAmountCleared * (pRowAmountCleared + 1) / 2;
		UpdateScoreField();
	}

	public void PieceSpawned()
	{
		score += 1;
		UpdateScoreField();
	}

	private void UpdateScoreField()
	{
		scoreField.text = score.ToString().PadLeft(5,'0');
	}
}
