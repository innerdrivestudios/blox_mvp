using InnerDriveStudios.Util;
using UnityEngine;

/**
 * This class listens to the TetrisGameManager so that it can display a queue piece.
 * 
 * The idea is that you:
 * 1) Setup a queue size in the TetrisGameManager
 * 2) Create the same amount of PiecePreview instances in the Scene as the queue size
 * 3) Link the PiecePreview instances together (eg Piece2 -> Piece1, Piece1 -> null)
 * 4) Set up the TetrisGameManager.OnPiecePreview event to link to the last preview piece (Preview2)
 * 
 * This will automatically fill the visual previews with the elements from the queue.
 */
public class PiecePreview : MonoBehaviour
{
	private TetrisPiece lastPiecePrefab = null;
	private TetrisPiece lastPieceInstance = null;

	public PiecePreview successor = null;

	/**
	 * Pass in a PREFAB of the TetrisPiece you want to preview.
	 */
    public void UpdatePiece (TetrisPiece pTetrisPiece)
	{
		//Since this is easy to get wrong, post a warning if the piece passed in is not a prefab
		Debug.Assert(!pTetrisPiece.gameObject.scene.IsValid(), "Piece must a prefab!");

		//clear the last piece instance
		if (lastPieceInstance != null) Destroy(lastPieceInstance.gameObject);
		//pass on the last piece prefab
		if (lastPiecePrefab != null && successor != null) successor.UpdatePiece(lastPiecePrefab);

		//create the new piece
		TetrisPiece newPiece;
		newPiece = Instantiate(pTetrisPiece, transform);

		//center it on me based on it bounds
		Bounds bounds;
		Common.GetBounds(newPiece.transform, out bounds);
		//bounds are in worldspace but we need local
		Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

		newPiece.transform.localPosition = -localCenter;
		newPiece.transform.localRotation = Quaternion.identity;
		newPiece.transform.localScale = Vector3.one;

		lastPieceInstance = newPiece;
		lastPiecePrefab = pTetrisPiece;
	}

}
