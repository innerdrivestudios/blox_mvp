using UnityEngine;

/**
 * A BloxPiece is a container for little cublets.
 * These have a specific position to ensure the rotation pivot of the whole BloxPiece is correct,
 * BUT this conflicts with having a single spawn position in the scene, that is why each BloxPiece
 * defines its own spawnOffset.
 * 
 * In addition each piece has a ghost, a preview that allows you to see where the piece would end up 
 * if you drop it now.
 */
public class BloxPiece : MonoBehaviour
{
    [field: SerializeField] public Vector2 spawnOffset  { get; private set; }
    [field: SerializeField] public GameObject ghost     { get; private set; }
}
