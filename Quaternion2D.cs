using UnityEngine;
using System.Collections;

public class Quaternion2D {
    float mSin;
    float mCos;

    public float euler {
        get {
            return Mathf.Atan2(mSin, mCos) * Mathf.Rad2Deg;
        }
        set {
            float radAngle = Mathf.Deg2Rad * value;
            mSin = Mathf.Sin(radAngle);
            mCos = Mathf.Cos(radAngle);
        }
    }

    public Quaternion2D() {
        mCos = 1.0f;
        mSin = 0.0f;
    }

    public Quaternion2D(float x, float y) {
        float l = Mathf.Sqrt(x * x + y * y);
        mCos = x / l;
        mSin = y / l;
    }

    public Quaternion2D(Vector2 Direction) : this(Direction.x, Direction.y) { }

    public Quaternion2D(Vector2 From, Vector2 To) : this(To - From) { }

    public Quaternion2D(float Angle) {
        float radAngle = Mathf.Deg2Rad * Angle;
        mSin = Mathf.Sin(radAngle);
        mCos = Mathf.Cos(radAngle);
    }

    public Quaternion2D(Quaternion2D e) {
        mCos = e.mCos;
        mSin = e.mSin;
    }

    public Vector2 GetVector(float Length = 1) {
        return new Vector2(mCos * Length, mSin * Length);
    }

    public Vector2 GetNormalVector(float Length = 1) {
        return new Vector2(-mSin * Length, mCos * Length);
    }

    public Quaternion2D GetNormal() {
        return new Quaternion2D(-mSin, mCos);
    }

    public void Rotate(Quaternion2D e) {
        float newCos = mCos * e.mCos - mSin * e.mSin;
        float newSin = mCos * e.mSin + mSin * e.mCos;

        mCos = newCos;
        mSin = newSin;
    }

    public void Rotate(float Angle) {
        Rotate(new Quaternion2D(Angle));
    }

    public void Inverse() {
        mSin = -mSin;
    }

    public void InverseRotate(Quaternion2D e) {
        float newCos = mCos * e.mCos + mSin * e.mSin;
        float newSin = mSin * e.mCos - mCos * e.mSin;

        mCos = newCos;
        mSin = newSin;
    }

    public void InverseRotate(float Angle) {
        InverseRotate(new Quaternion2D(Angle));
    }
    
    static public Quaternion2D Rotate(Quaternion2D Original, Quaternion2D Angle) {
        Quaternion2D res = new Quaternion2D(Original);
        res.Rotate(Angle);
        return res;
    } 

    static public Quaternion2D InverseRotate(Quaternion2D Original, Quaternion2D Angle) {
        Quaternion2D res = new Quaternion2D(Original);
        res.InverseRotate(Angle);
        return res;
    }

    static public Quaternion2D Inverse(Quaternion2D Original) {
        Quaternion2D res = new Quaternion2D(Original);
        res.Inverse();
        return res;
    }

    static public float ToEuler(Quaternion2D Angle) {
        return Angle.euler;
    }

    static public float CosBetween(Quaternion2D a, Quaternion2D b) {
        return a.mCos * b.mCos + a.mSin * b.mSin;
    }

    static public Quaternion2D MiddleBetween(Quaternion2D from, Quaternion2D to) {
        Quaternion2D res = new Quaternion2D(from.mCos + to.mCos, from.mSin + to.mSin);
        if (CosBetween(from.GetNormal(), to) < 0) {
            res.mSin = -res.mSin;
            res.mCos = -res.mCos;
        }
        return res;
    }

    public static explicit operator Quaternion(Quaternion2D e) {
        return new Quaternion(e.mCos, e.mSin, 0, 0);
    }

    public static Quaternion2D operator*(Quaternion2D a, Quaternion2D b) {
        return Rotate(a, b);
    }

    public static Quaternion2D operator/(Quaternion2D a, Quaternion2D b) {
        return InverseRotate(a, b);
    }

    public static Quaternion2D operator+(Quaternion2D a, Quaternion2D b) {
        return MiddleBetween(a, b);
    }

    public static bool operator==(Quaternion2D a, Quaternion2D b) {
        return a.mCos == b.mCos && a.mSin == b.mSin;
    }

    public static bool operator!=(Quaternion2D a, Quaternion2D b) {
        return a.mCos != b.mCos || a.mSin != b.mSin;
    }
}
