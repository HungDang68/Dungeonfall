using UnityEngine;
using Mirror;

public class SpinningProjectile : NetworkBehaviour
{
    private Transform transf;
    private int currentZRotation;
    void Start()
    {
        transf = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        CmdSetFlipZ(15);
    }


    private void CmdSetFlipZ(int flipZ)
    {
        // OnFlipZChanged(flipZ);

        currentZRotation -= flipZ;
        transf.rotation = Quaternion.Euler(0,
                                     0,
                                     currentZRotation);
    }

    [ClientRpc]//Client call this
    private void OnFlipZChanged(int flipZ)
    {
        currentZRotation -= flipZ;
        transf.rotation = Quaternion.Euler(0,
                                     0,
                                     currentZRotation);
    }
}
