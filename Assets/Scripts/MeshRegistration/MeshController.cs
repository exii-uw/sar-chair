////#define DEBUG_CLOUD_1
////#define DEBUG_CLOUD_2

//using System;
//using System.Threading;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using MathNet.Numerics.LinearAlgebra;
//using System.Threading.Tasks;



//public class MeshController : MonoBehaviour
//{
//    public struct TransformationParameters
//    {
//        public Quaternion R;
//        public Vector3 t;
//    }

//    private delegate void TransformProgressUpdate(TransformationParameters _tparams);
//    public GameObject MeshGenPrefab;


//    // Parameters that can be adjusted to fine-tune algorithm
//    [Range(0.0f, 1.0f)]
//    public float MaxDistanceThreshold = 0.1f; // Threshold of 10 cm

//    [Range(0.0f, 1.0f)]
//    public float RMSErrorThreshold = 0.1f;
    
//    [Range(0, 100)]
//    public int MaxIterations = 10;

//    [Range(0.0f, 1.0f)]
//    public float SubSamplePrecent = 0.5f;

//    public bool EnableVisualFeedback = false;
//    private GameObject IndicatorSphere;

//    // Stores list of meshes
//    private List<GameObject> m_MeshObjects = new List<GameObject>();

//    // Locks List objects when finding intersection of meshess
//    private System.Object m_intersectionListLock = new System.Object();


//    private System.Object m_registeredOffsetTransformLock = new System.Object();
//    private AutoResetEvent m_RegistrationComplete = new AutoResetEvent(false);
//    private AutoResetEvent m_ProgressRegistrationComplete = new AutoResetEvent(false);
//    private TransformationParameters m_registeredOffsetTransform;

//    private bool m_ProcessingFlag = false;
//    private TransformationParameters prevSourceT;


//    // Start is called before the first frame update
//    void Start()
//    {


//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (EnableVisualFeedback && IndicatorSphere == null)
//        {
//            IndicatorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//            IndicatorSphere.transform.parent = transform;
//            IndicatorSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//            IndicatorSphere.GetComponent<Renderer>().material.color = Color.green;
//        }


//        // Apply update for visulization
//        if (m_ProgressRegistrationComplete.WaitOne(0))
//        {
//            // Get Source and reset before applying 
//            GameObject Source = m_MeshObjects[m_MeshObjects.Count - 1];

//            lock (m_registeredOffsetTransformLock)
//            {
//                Source.transform.position = InverseTransformPoint(m_registeredOffsetTransform, prevSourceT.t);
//                Source.transform.rotation = Quaternion.Inverse(m_registeredOffsetTransform.R) * prevSourceT.R;
//            }
//        }

//        // Apply Offset once complete
//        if (m_RegistrationComplete.WaitOne(0))
//        {
//            m_ProcessingFlag = false;

//            if (EnableVisualFeedback)
//                IndicatorSphere.GetComponent<Renderer>().material.color = Color.green;

//            // Get Source and reset before applying 
//            GameObject Source = m_MeshObjects[m_MeshObjects.Count - 1];

//            lock (m_registeredOffsetTransformLock)
//            { 
//                Source.transform.position = InverseTransformPoint(m_registeredOffsetTransform, prevSourceT.t);
//                Source.transform.rotation = Quaternion.Inverse(m_registeredOffsetTransform.R) * prevSourceT.R;
//            }
//        }
//    }


//    ///////////////////////////////////////////////////////////////
//    /// PUBLIC INTERFACE
//    ///////////////////////////////////////////////////////////////

//    public void InstantiateMeshObject(int _samples, FiltedPointCloudCallbacks.SnapShotCallback callback = null)
//    {
//        if (_samples < 0 || _samples > 100)
//        {
//            throw new Exception("Sample amount is out of range");
//        }

//        GameObject obj = GameObject.Instantiate(MeshGenPrefab);
//        obj.transform.name = "MeshGen_" + m_MeshObjects.Count.ToString();
//        obj.transform.parent = transform;
//        obj.transform.position = transform.position;
//        obj.transform.rotation = transform.rotation;

//        obj.GetComponent<FilteredStaticPointCloud>().SampleFrameAmount = _samples;
//        obj.GetComponent<FilteredStaticPointCloud>().InitializeMesh(callback);

//        m_MeshObjects.Add(obj);
//    }


//    public List<GameObject> GetAcquiredMeshObjects()
//    {
//        return m_MeshObjects;
//    }


//    // Register previous two meshs (Count - 2) is Model mesh
//    public void RegisterMeshObjects()
//    {
//        if (m_MeshObjects.Count < 2)
//        {
//            throw new Exception("Insufficient mesh objects aquired");
//        }

//        RegisterMeshObjects(m_MeshObjects.Count - 2, m_MeshObjects.Count - 1);
//    }

//    // Register mesh objects of model and source index into mesh array
//    public void RegisterMeshObjects(int _modelIndex, int _sourceIndex)
//    {
//        if (_modelIndex < 0 || _modelIndex >= m_MeshObjects.Count ||
//            _sourceIndex < 0 || _sourceIndex >= m_MeshObjects.Count)
//        {
//            throw new Exception("Index out of range of mesh objects");
//        }

//        if (!m_ProcessingFlag)
//        {
//            if (EnableVisualFeedback)
//                IndicatorSphere.GetComponent<Renderer>().material.color = Color.red;

//            m_ProcessingFlag = true;
//            GameObject Model = m_MeshObjects[_modelIndex];
//            GameObject Source = m_MeshObjects[_sourceIndex];

//            TransformationParameters ModelTransform;
//            ModelTransform.R = Model.transform.rotation;
//            ModelTransform.t = Model.transform.position;
//            Vector3[] ModelLocalPoints = Model.GetComponent<FilteredStaticPointCloud>().vertices;

//            TransformationParameters SourceTransform;
//            SourceTransform.R = Source.transform.rotation;
//            SourceTransform.t = Source.transform.position;
//            Vector3[] SourceLocalPoints = Source.GetComponent<FilteredStaticPointCloud>().vertices;

//            prevSourceT.R = Source.transform.rotation;
//            prevSourceT.t = Source.transform.position;

//            var processTransform = Task.Run(() =>
//            {
//                TransformationParameters finalTransform = FindMeshTranform(
//                    ModelTransform,
//                    ModelLocalPoints,
//                    SourceTransform,
//                    SourceLocalPoints,
//                    (TransformationParameters _tparams) =>
//                    {
//                        lock (m_registeredOffsetTransformLock)
//                        {
//                            m_registeredOffsetTransform = _tparams;
//                            m_ProgressRegistrationComplete.Set();
//                        }
//                    },
//                    EnableVisualFeedback);

//                lock (m_registeredOffsetTransformLock)
//                {
//                    m_registeredOffsetTransform = finalTransform;
//                    m_RegistrationComplete.Set();
//                }
//            });

//        }
//    }



//    ///////////////////////////////////////////////////////////////
//    /// PRIVATE
//    ///////////////////////////////////////////////////////////////


//    // Optimize Game Mesh Objects
//    private TransformationParameters FindMeshTranform(
//        TransformationParameters ModelTransform, 
//        Vector3[] ModelLocalPoints,
//        TransformationParameters SourceTransform,
//        Vector3[] SourceLocalPoints,
//        TransformProgressUpdate ProgressUpdate, 
//        bool EnableProgressUpdate = false
//        )
//    {
//        //  Convert data to homogeneous coordinates. NOTE: Unity does not do this for you. Why?
//        TransformationParameters modelT = ModelTransform;
//        int modelCount = Mathf.FloorToInt(ModelLocalPoints.Length * SubSamplePrecent);
//        Vector4[] model = new Vector4[modelCount];

//        Parallel.For(0, model.Length, i => {
//            int index = Mathf.FloorToInt((1.0f / SubSamplePrecent) * i);
//            model[i] = TransformPoint(modelT, ModelLocalPoints[index]);
//            model[i].w = 1.0f;
//        });

//        TransformationParameters sourceT = SourceTransform;
//        int sourceCount = Mathf.FloorToInt(SourceLocalPoints.Length * SubSamplePrecent);
//        Vector4[] source = new Vector4[sourceCount];

//        Parallel.For(0, source.Length, i => {
//            int index = Mathf.FloorToInt((1.0f / SubSamplePrecent) * i);
//            source[i] = TransformPoint(sourceT, SourceLocalPoints[i]);
//            source[i].w = 1.0f;
//        });


//        // Preprocess data and remove all point cloud data that is further away then some threshold N. 
//        List<Vector4> M_pruned = new List<Vector4>();
//        List<Vector4> S_pruned = new List<Vector4>();


//        Parallel.For(0, source.Length, i => {

//            Vector4 s_i = source[i];
//            Vector4 p = new Vector4();

//            float minFound = float.PositiveInfinity;

//            for (int j = 0; j < model.Length; ++j)
//            {
//                Vector4 m_i = model[j];
//                var d = (s_i - m_i).magnitude;
//                if (d > MaxDistanceThreshold) continue;
//                if (d < minFound)
//                {
//                    minFound = d;
//                    p = m_i;
//                }
//            }


//            if (minFound < MaxDistanceThreshold)
//            {
//                lock (m_intersectionListLock)
//                {
//                    M_pruned.Add(p);
//                    S_pruned.Add(s_i);
//                }
//            }



//        });

      

//        // initial parameters
//        var parameters0 = new Matrix(6, 1);
//        System.Random autoRand = new System.Random();

//        bool registered = false;
//        int iter = 0;
//        while (!registered)
//        {
//            Matrix4x4 TransformMat = ConvertParametersToMatrix4(parameters0);

//            // Find Closest Points
//            Vector4[] X = new Vector4[S_pruned.Count];
//            Parallel.For(0, S_pruned.Count, i => {

//                Vector4 s_i = TransformMat.inverse * S_pruned[i];
//                Vector4 p = new Vector4();

//                float minFound = float.PositiveInfinity;

//                for (int j = 0; j < M_pruned.Count; ++j)
//                {
//                    Vector4 m_i = M_pruned[j];
//                    var d = (s_i - m_i).magnitude;
//                    if (d < minFound)
//                    {
//                        minFound = d;
//                        p = m_i;
//                    }
//                }

//                if (p == Vector4.zero)
//                    throw new System.Exception("Value for model pick should not be zero!");

//                X[i] = p;
//            });


//            // Run LM 
//            int nvalues = S_pruned.Count * 3;
//            LevenbergMarquardt.Function f = delegate (Matrix parameters)
//            {
//                var error = new Matrix(nvalues, 1);
//                Matrix4x4 Tn = ConvertParametersToMatrix4(parameters);

//                Parallel.For(0, S_pruned.Count, i => {
//                    Vector4 m_i = X[i];
//                    Vector4 s_i = S_pruned[i];

//                    Vector4 s_i_star = Tn.inverse * s_i;
//                    Vector4 p = m_i - s_i_star;
//                    error[i * 3] = p.x;
//                    error[i * 3 + 1] = p.y;
//                    error[i * 3 + 2] = p.z;
//                });


//                return error;
//            };

//            LevenbergMarquardt levenbergMarquardt = null;
//            if (EnableProgressUpdate)
//            {
//                levenbergMarquardt = new LevenbergMarquardt(f, (Matrix parameter) =>
//                {
//                // Updates each iteration of LM loop
//                TransformationParameters tparams = ConvertParameters(parameter);
//                    ProgressUpdate(tparams);
//                });
//            }
//            else
//            {
//                levenbergMarquardt = new LevenbergMarquardt(f, null);
//            }

//            //var levenbergMarquardt = new LevenbergMarquardt(f, null);

//            levenbergMarquardt.maximumIterations = 100;
//            levenbergMarquardt.lambdaIncrement = 10.0f;
//            levenbergMarquardt.minimumReduction = 1.0e-9;
//            levenbergMarquardt.initialLambda = 0.001;
//            levenbergMarquardt.minimumErrorTolerance = RMSErrorThreshold;

//            var rmsError = levenbergMarquardt.Minimize(parameters0);
//            Debug.Log("LM (" + iter.ToString() + "): " + rmsError.ToString() + "    " + levenbergMarquardt.State.ToString());
          

//            if (rmsError < RMSErrorThreshold || iter > MaxIterations)
//            {
//                registered = true;
//            }

//            iter++;
//        }



//#region DEBUG_SPHERE_2
//#if DEBUG_CLOUD_2
//        // Transform Object
//        {
//            Vector3 t = new Vector3((float)parameters0[0], (float)parameters0[1], (float)parameters0[2]);
//            Vector3 e = new Vector3((float)parameters0[3], (float)parameters0[4], (float)parameters0[5]);
//            Quaternion q = Quaternion.Euler(e);

//            Matrix4x4 Tn = Matrix4x4.TRS(
//                t,
//                q,
//                Vector3.one
//                );

//            Debug.Log("LM Iteration");
//            Debug.Log("Translation: " + t.ToString());
//            Debug.Log("Euler: " + e.ToString());
//            {
//                int indexS = 0;
//                GameObject debugIntersection = new GameObject();
//                debugIntersection.name = "DebugTransformedPoints_A";
//                foreach (var v in S_pruned)
//                {
//                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//                    sphere.name = "Sphere_" + indexS.ToString();
//                    sphere.transform.position = Tn.inverse * v;
//                    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
//                    sphere.transform.parent = debugIntersection.transform;
//                    sphere.GetComponent<Renderer>().material.color = Color.blue;
//                }
//            }

//            {
//                int indexS = 0;
//                GameObject debugIntersection = new GameObject();
//                debugIntersection.name = "DebugTransformedPoints_W";
//                foreach (var v in M_pruned)
//                {
//                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//                    sphere.name = "Sphere_" + indexS.ToString();
//                    sphere.transform.position = v;
//                    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
//                    sphere.transform.parent = debugIntersection.transform;
//                    sphere.GetComponent<Renderer>().material.color = Color.green;
//                }
//            }
//        }
//#endif
//        #endregion


//        return ConvertParameters(parameters0);
//    }


//    // Covnert parameters into Transformation Matrix
//    private Matrix4x4 ConvertParametersToMatrix4(Matrix _params)
//    {
//        Vector3 t = new Vector3((float)_params[0], (float)_params[1], (float)_params[2]);
//        Vector3 e = new Vector3((float)_params[3], (float)_params[4], (float)_params[5]);
//        Quaternion q = Quaternion.Euler(e);

//        Matrix4x4 T = Matrix4x4.TRS(
//            t,
//            q,
//            Vector3.one
//            );

//        return T;
//    }

//    private Vector3 InverseTransformPoint(TransformationParameters _param, Vector3 _p)
//    {
//        return -(Quaternion.Inverse(_param.R) * _param.t) + Quaternion.Inverse(_param.R) * _p;
//    }

//    private Vector3 TransformPoint(TransformationParameters _param, Vector3 _p)
//    {
//        return  _param.t + _param.R * _p;
//    }

//    private TransformationParameters ConvertParameters(Matrix _params)
//    {
//        TransformationParameters T;

//        T.t = new Vector3((float)_params[0], (float)_params[1], (float)_params[2]);
//        Vector3 e = new Vector3((float)_params[3], (float)_params[4], (float)_params[5]);
//        T.R = Quaternion.Euler(e);

//        return T;
//    }

//}

