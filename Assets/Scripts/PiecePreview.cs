using InnerDriveStudios.Util;
using UnityEngine;

/**
 * This class listens to the BloxGameManager so that it can display a queue piece.
 * 
 * The idea is that you:
 * 1) Setup a queue size in the BloxGameManager
 * 2) Create the same amount of PiecePreview instances in the Scene as the queue size
 * 3) Link the PiecePreview instances together (eg Piece2 -> Piece1, Piece1 -> null)
 * 4) Set up the BloxGameManager.OnPiecePreview event to link to the last preview piece (Preview2)
 * 
 * This will automatically fill the visual previews with the elements from the queue.
 */
public class PiecePreview : MonoBehaviour
{
	private BloxPiece lastPiecePrefab = null;
	private BloxPiece lastPieceInstance = null;

	public PiecePreview successor = null;

	/**
	 * Pass in a PREFAB of the Piece you want to preview.
	 */
    public void UpdatePiece (BloxPiece pPiece)
	{
		//Since this is easy to get wrong, post a warning if the piece passed in is not a prefab
		Debug.Assert(!pPiece.gameObject.scene.IsValid(), "Piece must a prefab!");

		//clear the last piece instance
		if (lastPieceInstance != null) Destroy(lastPieceInstance.gameObject);
		//pass on the last piece prefab
		if (lastPiecePrefab != null && successor != null) successor.UpdatePiece(lastPiecePrefab);

		//create the new piece
		BloxPiece newPiece;
		newPiece = Instantiate(pPiece, transform);

		//center it on me based on it bounds
		Bounds bounds;
		Common.GetBounds(newPiece.transform, out bounds);
		//bounds are in worldspace but we need local
		Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

		newPiece.transform.localPosition = -localCenter;
		newPiece.transform.localRotation = Quaternion.identity;
		newPiece.transform.localScale = Vector3.one;

		lastPieceInstance = newPiece;
		lastPiecePrefab = pPiece;
	}

}
