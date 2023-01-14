﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

using UnityEngine;

using SLZ.Props;

namespace LabFusion.Network
{
    public class ConstraintCreateData : IFusionSerializable, IDisposable
    {
        public byte smallId;

        public ushort constrainerId;

        public Constrainer.ConstraintMode mode;

        public SerializedGameObjectReference tracker1;
        public SerializedGameObjectReference tracker2;

        public Vector3 point1;
        public Vector3 point2;

        public Vector3 normal1;
        public Vector3 normal2;

        public ushort point1Id;
        public ushort point2Id;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);

            writer.Write(constrainerId);

            writer.Write((byte)mode);

            writer.Write(tracker1);
            writer.Write(tracker2);

            writer.Write(point1);
            writer.Write(point2);

            writer.Write(normal1);
            writer.Write(normal2);

            writer.Write(point1Id);
            writer.Write(point2Id);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();

            constrainerId = reader.ReadUInt16();

            mode = (Constrainer.ConstraintMode)reader.ReadByte();

            tracker1 = reader.ReadFusionSerializable<SerializedGameObjectReference>();
            tracker2 = reader.ReadFusionSerializable<SerializedGameObjectReference>();

            point1 = reader.ReadVector3();
            point2 = reader.ReadVector3();

            normal1 = reader.ReadVector3();
            normal2 = reader.ReadVector3();

            point1Id = reader.ReadUInt16();
            point2Id = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ConstraintCreateData Create(byte smallId, ushort constrainerId, ConstrainerPointPair pair, ConstraintSyncable point1, ConstraintSyncable point2)
        {
            return new ConstraintCreateData()
            {
                smallId = smallId,
                constrainerId = constrainerId,
                mode = pair.mode,
                tracker1 = new SerializedGameObjectReference(pair.go1),
                tracker2 = new SerializedGameObjectReference(pair.go2),
                point1 = pair.point1,
                point2 = pair.point2,
                normal1 = pair.normal1,
                normal2 = pair.normal2,
                point1Id = point1.GetId(),
                point2Id = point2.GetId(),
        };
        }
    }

    [Net.DelayWhileLoading]
    public class ConstraintCreateMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ConstraintCreate;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ConstraintCreateData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (!data.tracker1.gameObject || !data.tracker2.gameObject)
                            return;

                        if (SyncManager.TryGetSyncable(data.constrainerId, out var syncable) && syncable is PropSyncable constrainer && constrainer.TryGetExtender<ConstrainerExtender>(out var extender)) {
                            var comp = extender.Component;
                            comp.mode = data.mode;

                            // Setup points
                            comp._point1 = data.point1;
                            comp._point2 = data.point2;

                            comp._normal1 = data.normal1;
                            comp._normal2 = data.normal2;

                            // Setup gameobjects
                            comp._gO1 = data.tracker1.gameObject;
                            comp._gO2 = data.tracker2.gameObject;
                            comp._rb1 = comp._gO1.GetComponentInChildren<Rigidbody>(true);
                            comp._rb2 = comp._gO2.GetComponentInChildren<Rigidbody>(true);
                            
                            // Create the constraint
                            ConstrainerPatches.IsReceivingConstraints = true;
                            ConstrainerPatches.FirstId = data.point1Id;
                            ConstrainerPatches.SecondId = data.point2Id;

                            comp.PrimaryButtonUp();

                            ConstrainerPatches.FirstId = 0;
                            ConstrainerPatches.SecondId = 0;
                            ConstrainerPatches.IsReceivingConstraints = false;
                        }
                    }
                }
            }
        }
    }
}
