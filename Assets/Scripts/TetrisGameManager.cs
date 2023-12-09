using InnerDriveStudios.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 * The TetrisGameManager manages the whole game loop of spawning pieces, detecting and clearing full rows etc.
 * 
 * The main principle is that we simply spawn pieces that consist of little cubes, 
 * and that by the positions of those little cubes to a 2d index into a grid,
 * we can check whether those cubes make up a full row, are hitting other cubes etc.
 * 
 * Note that the goal of this implementation was not to make it as optimized as possible, 
 * there is definitely room for improvement in that area, 
 * but to make the methods as small and readable as possible.
 * 
 * Most required events have already been implemented, but feel free to add more as required.
 * 
 * @author J.C. Wichman
 * 
 * Note: this code has been written for educational purposes and cannot be used commercially in any
 * way since I have no involvement with the Tetris company and don't own any of their copyrights.
 * It is a mental exercise and a tribute to the brilliant mind of Alexey Pajitnov who created Tetris in the 1980's.
 */
public class TetrisGameManager : MonoBehaviour
{
	[SerializeField] private TetrisPiece[] piecePrefabs;

	//Instead of constructing new pieces randomly directly, we push them through a queue,
	//that we prefill with a certain amount of pieces (might be zero)
	//since we need to push items both ways (related to the HOLD ability) we use a LinkedList as a queue
	LinkedList<TetrisPiece> pieceQueue = new();
	[SerializeField] private int pieceQueueSize = 2;

	[SerializeField] private float autoMoveDownInterval = 1;

	[SerializeField] private DropVFX dropVFXPrefab;

	[Header ("Game events")]

	[SerializeField] private UnityEvent OnInitialized;
	[SerializeField] private UnityEvent OnGameStart;
	[SerializeField] private UnityEvent<TetrisPiece> OnPieceQueued;
	[SerializeField] private UnityEvent<TetrisPiece> OnPieceHeld;
	[SerializeField] private UnityEvent<TetrisPiece> OnPieceSpawned;
	[SerializeField] private UnityEvent<int> OnRowsRemoved;
	[SerializeField] private UnityEvent OnGameOver;

	//these are the original tetris field size settings, if you change this,
	//make sure it is a multiple of two and at least 10*20
	private const int playingFieldWidth = 10;
	private const int playingFieldHeight = 20;

	//each piece cublet is stored in a cell in this grid, based on its local position
	//and the playfield cell size through the simple calculation: (int)(localPosition/cellsize)
	private Transform[,] grid = new Transform[playingFieldWidth, playingFieldHeight];

	//the reference point for spawning new pieces.
	//Pieces are spawned at this point + their spawnoffset
	private Vector3 newPieceSpawnPoint = new Vector3(playingFieldWidth / 2 - 0.5f, playingFieldHeight - 0.5f, 0);

	private bool inPlay = false;

	private TetrisPiece currentTetrisPiecePrefab = null;
	private Transform currentTetrisPieceInstance = null;
	private Transform currentTetrisPieceGhostInstance = null;

	//each time a new current piece has spawned, you can trigger the HOLD ability (once).
	//if you hold while there is NO held piece, the current piece is stored (held) and a new piece is generated
	//if you hold while there IS a held piece, the current piece is swapped with the held piece and held piece spawns on top
	private TetrisPiece heldPiecePrefab = null;
	private bool holdAbilityAvailable = true;

	//when pieces are stored, we keep track of the y range of rows that have changed so we can optimize full row checking
	private int minChangedY = -1;
	private int maxChangedY = -1;

	//we also keep track of the highest row that is filled, so we can optimize row shifting after removing full rows
	private int highestFilledY = -1;

	private void Awake()
	{
		//don't run the update loop by default
		enabled = false;
		OnInitialized.Invoke();
	}

	[ContextMenu("Start Game")]
	public void StartGame()
	{
		if (inPlay)
		{
			Debug.Log("Cannot start a game that hasn't ended yet.");
			return;
		}

		ResetGame();
		enabled = true;
		inPlay = true;

		StartCoroutine(GameLoopCoroutine());
	}

	private IEnumerator GameLoopCoroutine()
	{
		//make sure StartCoroutine continues first before actually starting this loop
		yield return null;

		OnGameStart.Invoke();

		PrefillPieceQueue();

		while (inPlay)
		{
			//if we don't have a piece and we can't create one, the game is over!
			if (!HasCurrentPiece())
			{
				if (CreateNewPiece())
				{
					OnPieceSpawned.Invoke(currentTetrisPiecePrefab);
				}
				else
				{
					inPlay = false;
					break;
				}
			}
			
			//we need to be able to interrupt this, that is why we use this approach instead of WaitForSeconds etc
			//interruption happens when the current piece is dropped instantaneously (by calling DropPiece)
			//which causes us to no longer have a piece
			float timeBeforeLoop = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - timeBeforeLoop < autoMoveDownInterval && HasCurrentPiece())
			{
				yield return null;
			}

			//if we still have a piece (maybe we dropped it!) move it down
			if (HasCurrentPiece()) MovePieceDown();	
		}

		inPlay = false;
		enabled = false;

		OnGameOver.Invoke();
	}

	/**
	 * Preselect x random piece prefabs depending on the size of the queue.
	 */
	private void PrefillPieceQueue()
	{
		for (int i = 0; i < pieceQueueSize; i++)
		{
			TetrisPiece piece = piecePrefabs.GetRandomElement();
			pieceQueue.AddLast(piece);
			OnPieceQueued.Invoke(piece);
		}
	}

	/**
	 * Gets a random piece from the piece prefabs, pushes it in the queue, 
	 * and returns the piece that 'falls' out of the queue.
	 */
	private TetrisPiece DequeuePiecePrefab()
	{
		//if we pushed back a HELD piece, the queue size is temporarily bigger than it is meant to be...
		bool useHeldPiece = pieceQueue.Count > pieceQueueSize;

		if (useHeldPiece)
		{
			//just use the held piece, don't update any previews etc
			TetrisPiece nextPiece = pieceQueue.First.Value;
			pieceQueue.RemoveFirst();
			return nextPiece;
		}
		else
		{
			//if the queue size is not overflowing, get a new random element but ....
			TetrisPiece nextPiece = piecePrefabs.GetRandomElement();

			//... actually check if we are using a queue at all!
			//if yes, add the new piece to the queue as the last entry,
			//and get the first element from the queue as the actual new piece to use
			//(if not we'll just use the value of nextPiece directly)
			if (pieceQueueSize > 0)
			{
				pieceQueue.AddLast(nextPiece);
				OnPieceQueued.Invoke(nextPiece);
				nextPiece = pieceQueue.First.Value;
				pieceQueue.RemoveFirst();
			}

			return nextPiece;
		}
	}

	private bool HasCurrentPiece()
	{
		return currentTetrisPiecePrefab != null;
	}

	/**
	 * Try to create a piece and place it on the 'battlefield'.
	 * @return whether the piece could be created and placed.
	 */
	private bool CreateNewPiece()
	{
		//we need to keep track of the original prefab we created the current piece from for the HOLD ability
		currentTetrisPiecePrefab = DequeuePiecePrefab();

		//create the piece using it's offset. Offset is needed because all the piece's elements need to 
		//be centered around it's rotation pivot
		currentTetrisPieceInstance = Instantiate(currentTetrisPiecePrefab, transform).transform;
		currentTetrisPieceInstance.localPosition = newPieceSpawnPoint + (Vector3)currentTetrisPiecePrefab.spawnOffset;

		//try to put the piece into the field and return whether that succeeded
		bool canBePlaced = TryToPlace(currentTetrisPieceInstance); 

		if (canBePlaced)
		{
			//and its ghost, turn it off until we have determined where it should go
			currentTetrisPieceGhostInstance = Instantiate(currentTetrisPiecePrefab.ghost, transform).transform;
			currentTetrisPieceGhostInstance.gameObject.SetActive(false);

			UpdateCurrentPieceGhost();
		}

		return canBePlaced;
	}

	/**
	 * Apply the given transformation, and return if the resulting block positions are free in the grid.
	 * If the resulting position aren't free in the grid, the requested transformation is undone.
	 */
	private bool TryToPlace(Transform pTetrisPiece, Vector3 pPositionOffset = default, float pRotationOffsetInDegrees = 0)
	{
		pTetrisPiece.localPosition += pPositionOffset;
		pTetrisPiece.Rotate(Vector3.forward, pRotationOffsetInDegrees);

		//assume we can place the piece, check every child cublet to see if we are wrong
		bool canPlace = true;
		foreach (Transform child in pTetrisPiece)
		{
			if (!IsGridLocationFree(GetGridIndex(child)))
			{
				canPlace = false;
				break;
			}
		}

		if (!canPlace)
		{
			//undo previous transforms
			pTetrisPiece.localPosition -= pPositionOffset;
			pTetrisPiece.Rotate(Vector3.forward, -pRotationOffsetInDegrees);
		}

		return canPlace;
	}

	private Vector2Int GetGridIndex(Transform pCublet)
	{
		//fastest way to get the local position of a cublet relative to the grid is to get the 
		//parent's localPosition plus it's own rotated localPosition, way faster then doing .position
		Vector3 gridPosition = pCublet.parent.localPosition + pCublet.parent.localRotation * pCublet.localPosition;

		gridPosition.x = Mathf.Round(gridPosition.x);
		gridPosition.y = Mathf.Round(gridPosition.y);
		gridPosition.z = Mathf.Round(gridPosition.z);

		return new Vector2Int((int)gridPosition.x, (int)gridPosition.y);
	}

	private bool IsGridLocationFree(Vector2Int pGridIndex)
	{
		if (pGridIndex.x >= 0 && pGridIndex.x < playingFieldWidth && pGridIndex.y >= 0 && pGridIndex.y < playingFieldHeight)
		{
			return grid[pGridIndex.x, pGridIndex.y] == null;
		}
		else
		{
			return false;
		}
	}

	private void StoreAndDestroyCurrentPiece()
	{
		//We want to store the current piece in the grid.
		//After that we need to check which rows are full and need to be dissolved.
		//The slow way would be to check EVERY row in the grid, 
		//the fast way is to keep track of which rows have been changed by storing the current piece exactly.
		//Since pieces can rotate, we deduct the correct row range on the fly while detaching the little cublets:

		//set up the extremes to any piece will automatically overwrite the min and max
		//note that the highestFilled variable should be maintained throughout the gameplay and not reset per piece,
		//so you don't see that here
		minChangedY = int.MaxValue;
		maxChangedY = int.MinValue;

		//we need to loop backward since we might be detaching children into the grid
		for (int i = currentTetrisPieceInstance.childCount - 1; i >= 0; i--)
		{
			//store child cublet into the grid and detach it from main tetronimo
			Transform child = currentTetrisPieceInstance.GetChild(i);
			Vector2Int gridIndex = GetGridIndex(child);
			grid[gridIndex.x, gridIndex.y] = child;
			child.SetParent(transform, true);

			//update some fields used for optimized row solving and moving
			minChangedY = Mathf.Min(minChangedY, gridIndex.y);
			maxChangedY = Mathf.Max(maxChangedY, gridIndex.y);
			highestFilledY = Mathf.Max(highestFilledY, maxChangedY);
		}

		DestroyCurrentPiece();
		
		//after storing a piece the hold ability becomes available again
		holdAbilityAvailable = true;
	}

	private void DestroyCurrentPiece()
	{
		if (currentTetrisPieceInstance != null)
		{
			Destroy(currentTetrisPieceInstance.gameObject);
			currentTetrisPieceInstance = null;
		}

		if (currentTetrisPieceGhostInstance != null)
		{
			Destroy(currentTetrisPieceGhostInstance.gameObject);
			currentTetrisPieceGhostInstance = null;
		}

		currentTetrisPiecePrefab = null;
	}

	private void DissolveFullRows()
	{
		//the minChangedY and maxChangedY have been set up by the StoreCurrentPiece method
		//so we need to check every from minChangedY to maxChangedY, delete it if it is full 
		//and then shift down what is on top of it

		//The easiest way to do this is PER row and from TOP to BOTTOM.
		//Why? 
		//- PER ROW			-> since we don't have to deal with disjunct blocks of rows
		//					(theoretically it is possible to land the L and then have row 1, 2 and 4 being full, but not 3)

		//- TOP 2 BOTTOM -> Let's say we DON'T do this, then:
		//					If we clear row 5-6 (for example), starting with 5,
		//					we shift everything op top of it (6-20) down, in other words row 6 is now row 5.
		//					So IF we shift we need to adjust the next row index.

		//				 -> If we DO do this, then:
		//					If we clear row 6-5 (for example), we shift everything op top of it (7-20) down.
		//					in other words row 7 is now row 6. But since we are moving backward we don't care,
		//					since the next row we want to delete is row 5 which was untouched by the shift.

		//Moral of the story: lots of comments, short loop ;)
		int rowsRemoved = 0;

		for (int y = maxChangedY; y >= minChangedY; y--)
		{
			if (IsRowFull(y))
			{
				rowsRemoved++;
				DeleteRow(y);
			}
		}

		OnRowsRemoved.Invoke(rowsRemoved);
	}

	private bool IsRowFull(int pY)
	{
		for (int x = 0; x < playingFieldWidth; x++)
		{
			if (grid[x, pY] == null) return false;
		}

		return true;
	}

	private void DeleteRow(int pY)
	{
		//STEP 1 DESTROY THE ACTUAL CUBLETS
		for (int x = 0; x < playingFieldWidth; x++)
		{
			Destroy(grid[x, pY].gameObject);
			//We don't have to clear the row in the grid,
			//since overwriting the grid cells will be done by shifting all other rows down, see step 2
			//grid[x, pY] = null;
		}

		//STEP 2 OVERWRITE EVERY ROW WITH THE CONTENTS ABOVE IT
		for (int y = pY + 1; y <= highestFilledY; y++)
		{
			for (int x = 0; x < playingFieldWidth; x++)
			{
				//shift value down (overwriting anything that was in there)
				//for now we just move the object down, but you could change this into an animation later
				if (grid[x, y] != null) grid[x, y].localPosition += Vector3.down;
				grid[x, y - 1] = grid[x, y];
			}
		}

		//STEP 3 fill  the highest row that was filled with empty cells (since everything moved down)
		for (int x = 0; x < playingFieldWidth; x++)
		{
			grid[x, highestFilledY] = null;
		}

		//now one row less is filled, so reduce this optimization value by 1
		highestFilledY--;
	}

	private void UpdateCurrentPieceGhost()
	{
		//start by moving the ghost to the piece itself
		currentTetrisPieceGhostInstance.localPosition = currentTetrisPieceInstance.localPosition; 
		currentTetrisPieceGhostInstance.localRotation = currentTetrisPieceInstance.localRotation;

		//move the piece as far down as possible
		int moveDistance = 0;
		while (TryToPlace(currentTetrisPieceGhostInstance, Vector3.down)) {
			moveDistance++;
		}

		//if we actually moved it, turn it on, off otherwise
		currentTetrisPieceGhostInstance.gameObject.SetActive(moveDistance > 0);
	}

	private void ResetGame()
	{
		//destroy all gameobjects referenced in the grid
		if (grid != null)
		{
			for (int x = 0; x < playingFieldWidth; x++)
			{
				for (int y = 0; y < playingFieldHeight; y++)
				{
					if (grid[x, y] != null) Destroy(grid[x, y].gameObject);
				}
			}
		}

		////re initialize variables
		grid = new Transform[playingFieldWidth, playingFieldHeight];
		minChangedY = -1;
		maxChangedY = -1;
		highestFilledY = -1;

		//destroy any existing pieces in play if applicable
		DestroyCurrentPiece();
	}

	/**
	 * This method looks at the position of the current piece and its ghost and creates 
	 * a fading out gradient effect indicating movement from piece to ghost.
	 * The idea is you can use this to create a fade out effect after doing a drop.
	 */
	private void CreateDropVFX()
	{
		//we want to avoid create dropdown fx for stacked blocks (e.g the I tetromino vertically)
		//so what we do is that we loop through every block, store its x grid position to the block
		//and only replace the entry for an x if it's y is higher
		Dictionary<int, Transform> x2Cublet = new Dictionary<int, Transform>();

		for (int i = 0; i < currentTetrisPieceInstance.childCount; i++)
		{
			Transform cublet = currentTetrisPieceGhostInstance.GetChild(i);
			Vector2Int gridIndex = GetGridIndex(cublet);

			Transform currentCublet;
			if (x2Cublet.TryGetValue(gridIndex.x, out currentCublet))
			{
				Vector2Int currentCubletGridIndex = GetGridIndex(currentCublet);
				if (gridIndex.y > currentCubletGridIndex.y) x2Cublet[gridIndex.x] = currentCublet; 
			}
			else
			{
				x2Cublet[gridIndex.x] = cublet;
			}
		}

		//now that we have the highest cublets for each x, we'll create dropdown effects for each of them

		//get the movement delta for the whole piece
		Vector3 delta = currentTetrisPieceInstance.localPosition - currentTetrisPieceGhostInstance.localPosition;

		foreach (Transform cublet in x2Cublet.Values)
		{
			//the pivot of the DropVFX prefab should be at the bottom center
			Vector2Int gridIndex = GetGridIndex(cublet);
			DropVFX dropVFXInstance = Instantiate(dropVFXPrefab, transform);
			//align the bottom of the drop effect with the top of the ghost cublet
			dropVFXInstance.transform.localPosition = new Vector3(gridIndex.x, gridIndex.y+0.5f, 0);
			//and scale it so big it goes up to the pieces original cublet position
			dropVFXInstance.transform.localScale = new Vector3(1, delta.y, 1);
			//this is a little bit of a hack, it would be better if the currentPiecePrefab defines the color
			//since this assumes knowledge about how a tetromino is constructed (of sprites instead of meshes)
			dropVFXInstance.SetColor(cublet.GetComponent<SpriteRenderer>().color);
		}

	}

	/// ///////////////////////////////////////////////////////////////////////////
	///						PUBLIC MOVEMENT INTERFACE
	/// ///////////////////////////////////////////////////////////////////////////

	public void RotatePieceLeft() {
		if (!HasCurrentPiece()) return;

		TryToPlace(currentTetrisPieceInstance, default, -90);
		UpdateCurrentPieceGhost();
	}

	public void RotatePieceRight() {
		if (!HasCurrentPiece()) return;

		TryToPlace(currentTetrisPieceInstance, default, 90);
		UpdateCurrentPieceGhost();
	}

	public void MovePieceLeft() {
		if (!HasCurrentPiece()) return;

		TryToPlace(currentTetrisPieceInstance, Vector3.left, 0);
		UpdateCurrentPieceGhost();
	}

	public void MovePieceRight() {
		if (!HasCurrentPiece()) return;

		TryToPlace(currentTetrisPieceInstance, Vector3.right, 0);
		UpdateCurrentPieceGhost();
	}

	public void MovePieceDown() {
		if (!HasCurrentPiece()) return;

		if (!TryToPlace(currentTetrisPieceInstance, Vector3.down))
		{
			StoreAndDestroyCurrentPiece();
			DissolveFullRows();
		}
	}

	public void DropPiece()
	{
		if (!HasCurrentPiece()) return;

		//move as far down as we can and then immediately store the current piece
		CreateDropVFX();
		TryToPlace(currentTetrisPieceInstance, currentTetrisPieceGhostInstance.localPosition - currentTetrisPieceInstance.localPosition);
		StoreAndDestroyCurrentPiece();
		DissolveFullRows();
	}

	public void HoldPiece()
	{
		//hold only becomes available after landing/storing a piece
		if (!holdAbilityAvailable) return;
		if (!HasCurrentPiece()) return;

		//and make sure we can only do this once per completed piece
		holdAbilityAvailable = false;

		//store the current tetris piece prefab but hold on to what is currently held so we can swap them
		TetrisPiece temporary = heldPiecePrefab;
		heldPiecePrefab = currentTetrisPiecePrefab;
		OnPieceHeld.Invoke(heldPiecePrefab);

		//if the temporary not is null, push it back in the queue so
		//that it will be used to spawn the new piece instead of getting
		//a new piece from the queue, otherwise we will just store the piece 
		//and get a new piece from the queue
		if (temporary != null)
		{
			//insert the piece at the start of the queue
			pieceQueue.AddFirst(temporary);
		}

		//make sure the current piece is destroyed so the main game loop creates a new one
		//see DequeuePiecePrefab to understand how the .AddFirst line above triggers using the held piece
		DestroyCurrentPiece();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	///                                     DEBUG CODE
	////////////////////////////////////////////////////////////////////////////////////////////////

	/**
	 * Draw a grid of Gizmos for debugging, so we can see where all of the playing field 'cells' are
	 * and if their transform values match our actual pieces.
	 */

	Color normalCellColor = new Color(0, 1, 0, 0.5f);
	Color fillCellColor = new Color(1, 0, 0, 0.5f);
	Color spawnPointColor = new Color(0.3f, 1, 1, 0.5f);

	private void OnDrawGizmos()
	{
		if (grid == null) return;

		for (int x = 0; x < playingFieldWidth; x++)
		{
			for (int y = 0; y < playingFieldHeight; y++)
			{
				//make sure that the cubes are drawn relative to our own transform
				Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
				Gizmos.color = (grid[x, y] == null) ? normalCellColor:fillCellColor;
				Gizmos.DrawCube(new Vector3(x, y, 0) * 1, new Vector3(0.9f, 0.9f, 0.9f));

				Gizmos.color = spawnPointColor;
				Gizmos.DrawSphere(newPieceSpawnPoint, 0.2f);
			}
		}
	}


}


