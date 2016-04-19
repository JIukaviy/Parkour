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

        HandGrabber gameObjectHandGrabber = gameObject.GetComponent<HandGrabber>();
        HandGrabber instanceHandGrabber = instance.GetComponent<HandGrabber>();

        Action restoreGhost = null;
        Action onCreationEnd = null;

        if (gameObjectManipulator != null && instanceManipulator != null) {
            if (gameObjectHandGrabber != null && instanceHandGrabber != null) {
                restoreGhost = delegate () {
                    instanceManipulator.targetAngle = gameObjectManipulator.targetAngle;

                    if (gameObjectHandGrabber != null && instanceHandGrabber != null) {
                        if (!gameObjectHandGrabber.grabbed) {
                            instanceHandGrabber.Ungrab();
                        }
                        instanceHandGrabber.canGrab = gameObjectHandGrabber.canGrab;
                    }
                };
                onCreationEnd = delegate () {
                    GameObject ghost = GhostCreator.GetGhostByOriginal(gameObject);
                    Rigidbody2D ghostRigidBody = ghost == null ? null : ghost.GetComponent<Rigidbody2D>();
                    instanceHandGrabber.OnCopy(gameObjectHandGrabber, ghostRigidBody);
                };
            } else {
                restoreGhost = delegate () {
                    instanceManipulator.targetAngle = gameObjectManipulator.targetAngle;
                };
            }
            instanceManipulator.OnCopy(gameObjectManipulator);
        }

        GhostCreator.RegisterGhost(instance, gameObject, restoreGhost, onCreationEnd);

        foreach (Skeleton.Bone bone in Bone.childs) {
            CreateGhost(bone, instance.GetComponent<Rigidbody2D>());
        }
    }

    public void CreateGhost() {
        CreateGhost(mSkeleton.root, null);
    }
}
