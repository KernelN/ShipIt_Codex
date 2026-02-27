using UnityEngine;

namespace ShipIt
{
    public class MusicTriggerer : MonoBehaviour
    {
        [SerializeField] AudioClip track;

        public void Start() => MusicManager.inst?.RequestTrack(track);
    }
}
