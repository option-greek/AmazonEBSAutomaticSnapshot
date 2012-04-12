using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Threading;
using System.Configuration;

namespace AmazonEBSAutoSnapshot
{
    class Program
    {
        static void Main(string[] args)
        {
            string dur = ConfigurationManager.AppSettings["SnapshotDurationInHours"];
            string vol_id_str = ConfigurationManager.AppSettings["VolumeIDsToSnapshot"];
            string num_prev_snaps = ConfigurationManager.AppSettings["NumberOfPrevSnaps2Keep"];

            // prepare a list of volume IDs from input string
            ArrayList al = new ArrayList();
            string[] parts = vol_id_str.Split(",".ToCharArray());
            foreach(string s in parts)
            {
                if(s.Trim().Length > 0)
                    al.Add(s);
            }
            // start snapshot loop
            Snapper snap = new Snapper(Convert.ToInt32(dur), al, Convert.ToInt32(num_prev_snaps));
            snap.StartSnapshotting();
        }
    }
}
