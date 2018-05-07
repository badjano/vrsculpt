using UnityEngine;
using System.Collections;


[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

public class VoxelChunk : MonoBehaviour {
	
	private int _SizeZ;

    private bool shouldUpdate;

    private bool neighboursBuilt;

	[SerializeField] private bool _cloud;

    public float Size
	{
		get { return transform.localScale.z*(_SizeZ)*20.0f; }
	}

    private RenderTexture DensityVolume;
    private Color[] Colors;

    // Use this for initialization
	private void Start() {
    
        _SizeZ = VoxelCalculator._ChunkSizeZ;

        DensityVolume = new RenderTexture(_SizeZ + 4, _SizeZ + 4, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.sRGB);

        DensityVolume.volumeDepth = _SizeZ + 4;
        DensityVolume.isVolume = true;
        DensityVolume.enableRandomWrite = true;
        DensityVolume.filterMode = FilterMode.Point;
        DensityVolume.wrapMode = TextureWrapMode.Clamp;
        DensityVolume.Create();

        var MF = GetComponent<MeshFilter>();
        if (MF.sharedMesh == null)
            MF.sharedMesh = new Mesh();
        var MR = GetComponent<MeshRenderer>();
//        MR.material = VoxelCalculator.Instance._DefaultMaterial;

        VoxelCalculator.Instance.CreateEmptyVolume(DensityVolume, _SizeZ + 4);
    }

    // Update is called once per frame
    void Update() {
        //DrawVoxel(Color.red, transform.position);
        if ( shouldUpdate || _cloud )
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
	    if (neighboursBuilt) return;
	    var nameArr = this.name.Split('_');
	    var x = int.Parse(nameArr[1]);
	    var y = int.Parse(nameArr[2]);
	    var z = int.Parse(nameArr[3]);

	    for (var i = x-1; i < x+2; i++)
	    {
		    for (var j = y-1; j < y+2; j++)
		    {
			    for (var k = z-1; k < z+2; k++)
			    {
				    VoxelCalculator.Instance.CreateChunk(i, j, k);
			    }
		    }
	    }

	    neighboursBuilt = true;
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
        var startTime = Time.realtimeSinceStartup;

        var MR = GetComponent<MeshRenderer>();
        var MF = GetComponent<MeshFilter>();
	    
	    if (_cloud)
	    {
		    VoxelCalculator.Instance.CreateEmptyVolume(DensityVolume, _SizeZ + 4);
		    VoxelCalculator.Instance.CreateNoiseVolume(DensityVolume, transform.position, _SizeZ + 4);
		    VoxelCalculator.Instance.BuildChunkMesh(DensityVolume, MF.sharedMesh);
			Colors = new Color[MF.sharedMesh.vertexCount];
		    VoxelCalculator.Instance.PaintSphereCPU(Colors, transform, MF.sharedMesh);
		    return;
	    }

        //VoxelCalculator.Instance.CreateEmptyVolume(DensityVolume, _SizeZ + 4);
        VoxelCalculator.Instance.DrawSphere(DensityVolume, transform.localPosition, _SizeZ + 4);
        //VoxelCalculator.Instance.CreateNoiseVolume(DensityVolume, transform.position, _SizeZ + 4);
        VoxelCalculator.Instance.BuildChunkMesh(DensityVolume, MF.sharedMesh);
	    if (Colors == null || Colors.Length != MF.sharedMesh.vertexCount)
	    {
	    	Colors = new Color[MF.sharedMesh.vertexCount];
	    }
//        VoxelCalculator.Instance.PaintSphere(Colors, transform.localPosition, MF.sharedMesh, _SizeZ + 4);
        VoxelCalculator.Instance.PaintSphereCPU(Colors, transform, MF.sharedMesh);

        //GetComponent<MeshCollider>().sharedMesh = MF.sharedMesh;
        //Debug.Log("CHUNK CREATION TIME = " + (1000.0f*(Time.realtimeSinceStartup-startTime)).ToString()+"ms");
    }

	private void OnDestroy()
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
