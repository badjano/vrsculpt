using UnityEngine;
using System.Collections;


[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

public class VoxelChunk : MonoBehaviour {
	
	private int _SizeZ;

    private bool shouldUpdate;

    private bool neighboursBuilt;

    public float Size
	{
		get { return transform.localScale.z*(_SizeZ)*20.0f; }
	}

    private RenderTexture DensityVolume;


    // Use this for initialization
    void Start() {
        _SizeZ = VoxelCalculator._ChunkSizeZ;

        DensityVolume = new RenderTexture(_SizeZ + 4, _SizeZ + 4, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.sRGB);

        DensityVolume.volumeDepth = _SizeZ + 4;
        DensityVolume.isVolume = true;
        DensityVolume.enableRandomWrite = true;
        DensityVolume.filterMode = FilterMode.Point;
        DensityVolume.wrapMode = TextureWrapMode.Clamp;
        DensityVolume.Create();

        MeshFilter MF = GetComponent<MeshFilter>();

        if (MF.sharedMesh == null)
            MF.sharedMesh = new Mesh();

        MeshRenderer MR = GetComponent<MeshRenderer>();
        MR.material = VoxelCalculator.Instance._DefaultMaterial;

        VoxelCalculator.Instance.CreateEmptyVolume(DensityVolume, _SizeZ + 4);
    }

    // Update is called once per frame
    void Update() {
        //DrawVoxel(Color.red, transform.position);
        if ( shouldUpdate)
        {
            UpdateMesh();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.gameObject.name == "DrawingHand")
        {
            checkNeighbours();
            shouldUpdate = true;
        }
    }

    private void checkNeighbours()
    {
        if (!neighboursBuilt)
        {
            string[] nameArr = this.name.Split('_');
            int x = int.Parse(nameArr[1]);
            int y = int.Parse(nameArr[2]);
            int z = int.Parse(nameArr[3]);

            for (int i = x-1; i < x+2; i++)
            {
                for (int j = y-1; j < y+2; j++)
                {
                    for (int k = z-1; k < z+2; k++)
                    {
                        VoxelCalculator.Instance.CreateChunk(i, j, k);
                    }
                }
            }

            neighboursBuilt = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "DrawingHand")
        {
            shouldUpdate = false;
        }
    }

    public void UpdateMesh()
    {
        float startTime = Time.realtimeSinceStartup;

        MeshRenderer MR = GetComponent<MeshRenderer>();
        MeshFilter MF = GetComponent<MeshFilter>();

        //VoxelCalculator.Instance.CreateEmptyVolume(DensityVolume, _SizeZ + 4);
        VoxelCalculator.Instance.DrawSphere(DensityVolume, transform.localPosition, _SizeZ + 4);
        //VoxelCalculator.Instance.CreateNoiseVolume(DensityVolume, transform.position, _SizeZ + 4);
        VoxelCalculator.Instance.BuildChunkMesh(DensityVolume, MF.sharedMesh);

        //GetComponent<MeshCollider>().sharedMesh = MF.sharedMesh;
        //Debug.Log("CHUNK CREATION TIME = " + (1000.0f*(Time.realtimeSinceStartup-startTime)).ToString()+"ms");
    }

    void OnDestroy()
	{
		if (DensityVolume!=null){
			
			DensityVolume.Release();
		}
		
	}
	
	public void DrawVoxel(Color Col, Vector3 Pos, float Dur = 0.0f)
	{
		Vector3 A,B,C,D,E,F,G,H;
				
		A = Pos;
		
		B = A + transform.right*Size;
		C = A + transform.up*Size;
		D = A + transform.forward*Size;
	
		E = A + transform.right*Size + transform.forward*Size;
		F = A + transform.right*Size + transform.up*Size;
		
		G = A + transform.right*Size + transform.up*Size + transform.forward*Size;
		H = A + transform.up*Size + transform.forward*Size;
				
		Debug.DrawLine(A, B, Col,Dur);
		Debug.DrawLine(B, E, Col,Dur);
		Debug.DrawLine(E, D, Col,Dur);
		Debug.DrawLine(D, A, Col,Dur);
	
		Debug.DrawLine(C, F, Col,Dur);
		Debug.DrawLine(F, G, Col,Dur);
		Debug.DrawLine(G, H, Col,Dur);
		Debug.DrawLine(H, C, Col,Dur);
	
		Debug.DrawLine(A, C, Col,Dur);
		Debug.DrawLine(D, H, Col,Dur);
		Debug.DrawLine(E, G, Col,Dur);
		Debug.DrawLine(B, F, Col,Dur);
	}
}
