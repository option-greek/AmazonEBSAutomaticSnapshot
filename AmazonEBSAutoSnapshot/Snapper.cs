using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Threading;

namespace AmazonEBSAutoSnapshot
{
    class Snapper
    {
        int hours;
        int snaps2keep;
        ArrayList volumes;
        AmazonEC2Client aec;

        public Snapper(int DurationInHours, ArrayList VolumeIDs, int NumberOfSnapsToKeep)
        {
            aec = new AmazonEC2Client(new AmazonEC2Config());
            volumes = VolumeIDs;
            hours = DurationInHours;
            snaps2keep = NumberOfSnapsToKeep;
        }

        // snapshot loop
        public void StartSnapshotting()
        {
            while (true)
            {
                Thread.Sleep(new TimeSpan(hours, 0, 0));
                try
                {
                    TakeSnapShot();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Caught exception while taking snapshot: " + e.Message);
                    Console.WriteLine("Stacktrace: " + e.StackTrace);
                }
            }
        }

        // take a snapshot of each volume
        void TakeSnapShot()
        {
            foreach (string s in volumes)
            {
                string descr = "";
                CreateSnapshotRequest csr = new CreateSnapshotRequest();
                csr.VolumeId = s;
                descr += DateTime.UtcNow.ToString("dd MMM, HH:mm - " + s + " - backup");
                csr.Description = descr;
                
                Console.WriteLine("Taking snapshot for volume: " + s);

                aec.CreateSnapshot(csr);
                PruneSnapShots(s);
                Console.WriteLine("Completed taking snapshot for volume: " + s);
            }
        }

        // delete older snapshots to keep the list clean
        void PruneSnapShots(string volume_id)
        {
            try
            {
                DescribeSnapshotsRequest dsr = new DescribeSnapshotsRequest();
                Filter f = new Filter();
                f.Name = "volume-id";
                f.Value.Add(volume_id);
                dsr.Filter.Add(f);
                dsr.Owner = "self";
                var resp = aec.DescribeSnapshots(dsr);
                var list = resp.DescribeSnapshotsResult.Snapshot;
                list.Sort((Snapshot s1, Snapshot s2) => { return s2.StartTime.CompareTo(s1.StartTime); });

                if (list.Count > snaps2keep)
                {
                    for (int i = snaps2keep; i < list.Count; i++)
                    {
                        DeleteSnapshotRequest del_req = new DeleteSnapshotRequest();
                        del_req.SnapshotId = list[i].SnapshotId;
                        aec.DeleteSnapshot(del_req);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while pruning snapshots: " + e.Message);
                Console.WriteLine("Stacktrace: " + e.StackTrace);
            }
        }
    }
}
