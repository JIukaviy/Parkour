using UnityEngine;
using System.Collections.Generic;
using System;

public class SkeletonGhoster : GhostCreator.Ghoster {
    PhysicsModel mPhysicsModel;
    Skeleton mSkeleton;

    public SkeletonGhoster(PhysicsModel aPhysicsModel, Skeleton aSkeleton) {
        GhostCreator.RegisterGhoster(this);
        mPhysicsModel = aPhysicsModel;
        mSkeleton = aSkeleton;
    }

    public void CreateGhost(Skeleton.Bone Bone, Rigidbody2D Parent) {
        GameObject gameObject = mPhysicsModel.GetObjectByName(Bone.name);
        GameObject instance = GameObject.Instantiate(gameObject);

        HingeJoint2D joint = instance.GetComponent<HingeJoint2D>();
        if (joint != null) {
            joint.connectedBody = Parent;
        }
        Manipulator gameObjectManipulator = gameObject.GetComponent<Manipulator>();
        Manipulator instanceManipulator = instance.GetComponent<Manipulator>();

        Action restoreGhost = null;

        if (gameObjectManipulator != null && instanceManipulator != null) {
            restoreGhost = delegate () {
                instanceManipulator.SetReferenceAngle(gameObjectManipulator.referenceAngle, gameObjectManipulator.angle);
                instanceManipulator.targetAngle = gameObjectManipulator.targetAngle;
            };
            restoreGhost();
        }

        GhostCreator.RegisterGhost(instance, gameObject, restoreGhost);

        foreach (Skeleton.Bone bone in Bone.childs) {
            CreateGhost(bone, instance.GetComponent<Rigidbody2D>());
        }
    }

    public void CreateGhost() {
        CreateGhost(mSkeleton.root, null);
    }
}
