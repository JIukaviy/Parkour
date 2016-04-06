using UnityEngine;
using System.Collections.Generic;

public class ChainCreator : MonoBehaviour {

    public class ChainElement {
        public GameObject instance;
        public Vector2 offset;
        public float rotate;
        public JointAngleLimits2D angleLimits;
        public List<ChainElement> childs;
        public float length;
        public string name;

        public ChainElement(ChainElement Parent, GameObject Prefab, Vector2 ParentOffset, Vector2 Offset, float Rotate, JointAngleLimits2D AngleLimits, int Layer, string Name) {
            offset = Offset;
            rotate = Rotate;

            childs = new List<ChainElement>();

            instance = Instantiate(Prefab, Offset, new Quaternion(0, 0, 1, Rotate)) as GameObject;
            instance.layer = Layer;
            instance.AddComponent<Rigidbody2D>();
            instance.name = Name;
            name = Name;

            //TODO: получать реальный размер объекта
            length = Prefab.transform.localScale.y;

            if (Parent != null) {
                instance.AddComponent<HingeJoint2D>();

                HingeJoint2D joint_component = instance.GetComponent<HingeJoint2D>();
                joint_component.autoConfigureConnectedAnchor = false;
                joint_component.anchor = Offset;
                joint_component.connectedAnchor = ParentOffset;
                joint_component.connectedBody = Parent.instance.GetComponent<Rigidbody2D>();
                joint_component.limits = AngleLimits;

                angleLimits = AngleLimits;

                Parent.childs.Add(this);
            }
        }

        public ChainElement findByName(string Name) {
            if (instance != null && instance.name == Name) {
                return this;
            }

            foreach (ChainElement i in childs) {
                ChainElement findedChild = i.findByName(Name);
                if (findedChild != null) {
                    return findedChild;
                }
            }

            return null;
        }
    }
    
    public GameObject PelvisPrefab;
    public GameObject Spine1Prefab;
    public GameObject Spine2Prefab;
    public GameObject Spine3Prefab;
    public GameObject LeftShoulderPrefab;
    public GameObject LeftArmPrefab;
    public GameObject LeftHandPrefab;
    public GameObject RightShoulderPrefab;
    public GameObject RightArmPrefab;
    public GameObject RightHandPrefab;
    public GameObject HeadPrefab;
    public GameObject LeftHipPrefab;
    public GameObject LeftElbowPrefab;
    public GameObject LeftFootPrefab;
    public GameObject RightHipPrefab;
    public GameObject RightElbowPrefab;
    public GameObject RightFootPrefab;

    private ChainElement Root;

    JointAngleLimits2D NewAngleLimits(float min, float max) {
        JointAngleLimits2D res = new JointAngleLimits2D();
        res.min = min;
        res.max = max;
        return res;
    }

    private ChainElement GetRagdollChain(int Layer) {
        Vector2 StdOffsetY = new Vector2(0, 0.5f);
        Vector2 StdOffsetX = new Vector2(0.5f, 0);

        ChainElement Pelvis         = new ChainElement(null, PelvisPrefab, Vector2.zero, Vector2.zero, 0, NewAngleLimits(0, 0), Layer, "Pelvis");

        ChainElement Spine1         = new ChainElement(Pelvis,        Spine1Prefab,        -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -10 , 10 ), Layer, "Spine1");
        ChainElement Spine2         = new ChainElement(Spine1,        Spine2Prefab,        -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -10 , 10 ), Layer, "Spine2");
        ChainElement Spine3         = new ChainElement(Spine2,        Spine3Prefab,        -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -10 , 10 ), Layer, "Spine3");

        ChainElement Head           = new ChainElement(Spine3,        HeadPrefab,          -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -60 , 60 ), Layer, "Head");

        ChainElement LeftShoulder   = new ChainElement(Spine3,        LeftShoulderPrefab,  -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -100, 100), Layer, "LeftShoulder");
        ChainElement LeftArm        = new ChainElement(LeftShoulder,  LeftArmPrefab,        StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -170, 0  ), Layer, "LeftArm");
        ChainElement LeftHand       = new ChainElement(LeftArm,       LeftHandPrefab,       StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -10 , 10 ), Layer, "LeftHand");

        ChainElement RightShoulder  = new ChainElement(Spine3,        RightShoulderPrefab, -StdOffsetY,  StdOffsetY, 0, NewAngleLimits( -100, 100), Layer, "RightShoulder");
        ChainElement RightArm       = new ChainElement(RightShoulder, RightArmPrefab,       StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -170, 0  ), Layer, "RightArm");
        ChainElement RightHand      = new ChainElement(RightArm,      RightHandPrefab,      StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -10 , 10 ), Layer, "RightHand");

        ChainElement LeftHip        = new ChainElement(Pelvis,        LeftHipPrefab,        StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -100, 100), Layer, "LeftHip");
        ChainElement LeftElbow      = new ChainElement(LeftHip,       LeftElbowPrefab,      StdOffsetY, -StdOffsetY, 0, NewAngleLimits(  0   ,170), Layer, "LeftElbow");
        ChainElement LeftFoot       = new ChainElement(LeftElbow,     LeftFootPrefab,       StdOffsetY,  StdOffsetX, 0, NewAngleLimits( -10 , 10 ), Layer, "LeftFoot");

        ChainElement RightHip       = new ChainElement(Pelvis,        RightHipPrefab,       StdOffsetY, -StdOffsetY, 0, NewAngleLimits( -100, 100), Layer, "RightHip");
        ChainElement RightElbow     = new ChainElement(RightHip,      RightElbowPrefab,     StdOffsetY, -StdOffsetY, 0, NewAngleLimits( 0   , 170), Layer, "RightElbow");
        ChainElement RightFoot      = new ChainElement(RightElbow,    RightFootPrefab,      StdOffsetY,  StdOffsetX, 0, NewAngleLimits( -10 , 10 ), Layer, "RightFoot");

        return Pelvis;
    }

    void Start() {
        GetRoot();
    }

    public ChainElement GetRoot() {
        return Root == null ? Root = GetRagdollChain(LayerMask.NameToLayer("Character")) : Root;
    }
}
