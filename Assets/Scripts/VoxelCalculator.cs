using UnityEngine;
using System.Collections;

public class VoxelCalculator : Singleton<VoxelCalculator> {
	
	protected VoxelCalculator () {}

    public GameObject _ChunkObj;
	
	public ComputeShader _CShaderGenerator;
	
	public ComputeShader _CShaderBuilder;

	//Maximum buffer size (triangles)
	public int _MaxSize = 21660;
	public int _Trilinear = 1;
	public int _MultiSampling = 1;
	
	//Chunk size in Z dimmension. Used in CS Builder for depth iterations.
	public static int _ChunkSizeZ = 27;
	public float _Scale = 1.0f;
	
	public float _UVScale = 0.2f;

    public float Density = 1.0f;
    public float Contrast = 1.0f;

    public Vector3 Noise = new Vector3(7.18f, 6.32f, 6.95f);
    public Vector3 Translate = new Vector3(0.1f, 0f, 0.1f);
    public float Speed = 10.0f;

    public float NoiseStr = 4.0f;

    public bool Cap = true;

    public Material _DefaultMaterial;

    private GameObject chunksHolder;

    private void Start()
    {
        chunksHolder = GameObject.Find("ChunkHolder");

        int xCount = 3;
        int yCount = 3;
        int zCount = 3;

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                for (int k = 0; k < zCount; k++)
                {
                    CreateChunk(i, j, k);
                }
            }
        }
    }

	public void CreateChunk(int i, int j, int k)
    {
        string name = "chunk_" + i + "_" + j + "_" + k;
        GameObject chunk = GameObject.Find(name);
        if ( chunk == null )
        {
            if (chunksHolder != null)
            {
                chunk = Instantiate(_ChunkObj);
                chunk.transform.parent = chunksHolder.transform;
                chunk.transform.localPosition = new Vector3(i, j, k) * _ChunkSizeZ;
                chunk.transform.localRotation = Quaternion.identity;
                chunk.name = name;
            }
        }
    }

    public struct Poly
	{
		//Vertex A
		public float A1,A2,A3;
		//Vertex B
		public float B1,B2,B3;
		//Vertex C
		public float C1,C2,C3;
		
		//Normals
		public float NA1,NA2,NA3;
		public float NB1,NB2,NB3;
		public float NC1,NC2,NC3;
		
		//Colors
//		public float CA1,CA2,CA3;
//		public float CB1,CB2,CB3;
//		public float CC1,CC2,CC3;
		
	};
	
	
	public void CreateEmptyVolume(RenderTexture Volume, int iSize = 32)
	{
		
		int mgen_id = _CShaderGenerator.FindKernel("FillEmpty");

		_CShaderGenerator.SetTexture(mgen_id,"Result",Volume);
		
		_CShaderGenerator.Dispatch(mgen_id,1,1,iSize);

    }

    public void CreateNoiseVolume(RenderTexture Volume, Vector3 Pos, int iSize = 32)
    {
        float startTime = Time.realtimeSinceStartup;

        int mgen_id = _CShaderGenerator.FindKernel("Simplex3d");

        _CShaderGenerator.SetTexture(mgen_id, "Result", Volume);

        Vector3 Tpos = Pos + Translate * Time.time * Speed + this.transform.position;

        _CShaderGenerator.SetVector("_StartPos", new Vector4(Tpos.x, Tpos.y, Tpos.z, 0.0f));
        //_CShaderGenerator.SetFloat("_MyTime",Time.time*Speed);
        _CShaderGenerator.SetFloat("_Str", NoiseStr);
        _CShaderGenerator.SetBool("_Cap", Cap);

        _CShaderGenerator.SetFloat("_Density", Density);
        _CShaderGenerator.SetFloat("_Contrast", Contrast);

        _CShaderGenerator.SetFloat("_NoiseA", Noise.x * 0.0001f);
        _CShaderGenerator.SetFloat("_NoiseB", Noise.y * 0.0001f);
        _CShaderGenerator.SetFloat("_NoiseC", Noise.z * 0.0001f);

        _CShaderGenerator.SetFloat("_NoiseX", Time.time * 40f + 2.0f);
        _CShaderGenerator.SetFloat("_NoiseY", Time.time * 50f + 4.0f);
        _CShaderGenerator.SetFloat("_NoiseZ", Time.time * 60f + 8.0f);

        _CShaderGenerator.Dispatch(mgen_id, 1, 1, iSize);
        //Debug.Log("Noise generation time:  " + (1000.0f*(Time.realtimeSinceStartup-startTime)).ToString()+"ms");
    }

    public void DrawSphere(RenderTexture Volume, Vector3 Pos, int iSize = 32)
    {
        float startTime = Time.realtimeSinceStartup;

        int mgen_id = _CShaderGenerator.FindKernel("Sphere");

        _CShaderGenerator.SetTexture(mgen_id, "Result", Volume);

        Vector3 RelativePos = chunksHolder.transform.InverseTransformPoint(this.transform.position);

        Vector3 Tpos = Pos - RelativePos;

        Vector3 worldScale = transform.localScale;
        Transform parent = transform.parent;

        while (parent != null)
        {
            worldScale = Vector3.Scale(worldScale, parent.localScale);
            parent = parent.parent;
        }

        _CShaderGenerator.SetVector("_StartPos", new Vector4(Tpos.x, Tpos.y, Tpos.z, 0.0f));
        _CShaderGenerator.SetFloat("_Brush_Size", worldScale.x * 0.5f + 0.5f);

        _CShaderGenerator.Dispatch(mgen_id, 1, 1, iSize);
    }

    public void BuildChunkMesh(RenderTexture Volume, Mesh NewMesh)
	{
		if (Volume == null || NewMesh == null)
		{
			Debug.LogWarning("Can't build mesh '"+NewMesh+"' from '"+Volume+"' volume");
			return;
		}
		
		float startTime = Time.realtimeSinceStartup;
		
		Poly[] DataArray;

		DataArray = new Poly[_MaxSize];
		
		ComputeBuffer CBuffer = new ComputeBuffer(_MaxSize,72);
		
		//Set data to container
		CBuffer.SetData(DataArray);
		
		//Set container
		int id = _CShaderBuilder.FindKernel("CSMain");
		_CShaderBuilder.SetTexture(id,"input_volume", Volume);
		_CShaderBuilder.SetBuffer(id,"buffer", CBuffer);
		
		//Set parameters for building
		_CShaderBuilder.SetInt("_Trilinear",_Trilinear);
		_CShaderBuilder.SetInt("_Size",_ChunkSizeZ);
		_CShaderBuilder.SetInt("_MultiSampling",_MultiSampling);
		
		//Build!
		_CShaderBuilder.Dispatch(id,1,1,1);
		
		//Recieve data from container
		CBuffer.GetData(DataArray);
		
		//Debug.Log("Building time: " + (1000.0f*(Time.realtimeSinceStartup-startTime)).ToString()+"ms");
		
		//Construct mesh using received data
		
		int vindex = 0;
		
		int count = 0;
		
		//Count real data length
		for (count=0;count<_MaxSize; count++)
		{
			if (DataArray[count].A1 == 0.0f && DataArray[count].B1 == 0.0f && DataArray[count].C1 == 0.0 &&
				DataArray[count].A2 == 0.0f && DataArray[count].B2 == 0.0f && DataArray[count].C2 == 0.0 &&
				DataArray[count].A3 == 0.0f && DataArray[count].B3 == 0.0f && DataArray[count].C3 == 0.0)
			{
				
				break;
			}
		}
		//Debug.Log(count+" triangles got");
		
		Vector3[] vertices = new Vector3[count*3];
		int[] tris = new int[count*3];
		Vector2[] uvs = new Vector2[count*3];
		Vector3[] normals = new Vector3[count*3];
		Color[] colors = new Color[count*3];
		
		//Parse triangles
		for (int ix=0;ix<count; ix++)
		{
			
			Vector3 vPos;
			Vector3 vOffset = new Vector3(-30,-30,-30);
			//A1,A2,A3
			vPos = new Vector3(DataArray[ix].A1,DataArray[ix].A2,DataArray[ix].A3)+vOffset;
			vertices[vindex] = vPos*_Scale;
			normals[vindex] = new Vector3(DataArray[ix].NA1,DataArray[ix].NA2,DataArray[ix].NA3);
			tris[vindex] = vindex;
			uvs[vindex] = new Vector2 (vertices[vindex].z, vertices[vindex].x)*-_UVScale;
//			colors[vindex] = new Color(DataArray[ix].CA1, DataArray[ix].CA2, DataArray[ix].CA3);
			
			vindex++;
		
			//B1,B2,B3
			vPos =  new Vector3(DataArray[ix].B1,DataArray[ix].B2,DataArray[ix].B3)+vOffset;
			vertices[vindex] =vPos*_Scale;
			normals[vindex] = new Vector3(DataArray[ix].NB1,DataArray[ix].NB2,DataArray[ix].NB3);
			tris[vindex] = vindex;
			uvs[vindex] = new Vector2 (vertices[vindex].z, vertices[vindex].x)*-_UVScale;	
//			colors[vindex] = new Color(DataArray[ix].CB1, DataArray[ix].CB2, DataArray[ix].CB3);
			
			vindex++;
		
			//C1,C2,C3
			vPos = new Vector3(DataArray[ix].C1,DataArray[ix].C2,DataArray[ix].C3)+vOffset;
			vertices[vindex] =  vPos*_Scale;	
			normals[vindex] = new Vector3(DataArray[ix].NC1,DataArray[ix].NC2,DataArray[ix].NC3);
			tris[vindex] = vindex;
			uvs[vindex] = new Vector2 (vertices[vindex].z, vertices[vindex].x)*-_UVScale;	
//			colors[vindex] = new Color(DataArray[ix].CC1, DataArray[ix].CC2, DataArray[ix].CC3);
			
			vindex++;
		}
		
		//We have got all data and are ready to setup a new mesh!
		
		NewMesh.Clear();

		NewMesh.vertices = vertices;
		NewMesh.uv = uvs; //Unwrapping.GeneratePerTriangleUV(NewMesh);
		NewMesh.triangles = tris;
		NewMesh.normals = normals; //NewMesh.RecalculateNormals();
//		NewMesh.colors = colors;
		;
		
		CBuffer.Dispose();
	}
}
