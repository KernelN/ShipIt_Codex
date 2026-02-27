using UnityEngine;

namespace ShipIt
{
    public class MusicTriggerer : MonoBehaviour
    {
        [SerializeField] AudioClip track;

        public void TriggerTrack()
        {
            if (MusicManager.inst)
            {
                MusicManager.inst.RequestTrack(track);
            }
        }

        public void TriggerTrack(AudioClip newTrack)
        {
            if (MusicManager.inst)
            {
                MusicManager.inst.RequestTrack(newTrack);
            }
        }
    }
}
