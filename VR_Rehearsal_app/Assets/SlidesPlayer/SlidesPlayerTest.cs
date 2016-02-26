using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SlidesPlayer))]
public class SlidesPlayerTest : MonoBehaviour
{
    private SlidesPlayer _player
    { get { return GetComponent<SlidesPlayer>(); } }

    public float switchInterval = 30f;

    private void Start()
    {
        //RoomCenter.currRoom.presenter.OnPostMove += StartSlide;
        //RoomCenter.currRoom.presenter.OnPreMove += StopSlide;

        _player.Play();
        StartCoroutine(SwitchSlide_CR());
    }

    private void StartSlide(DestType dest)
    {
        if (dest != DestType.PODIUM)
            return;

        _player.Play();
        StartCoroutine(SwitchSlide_CR());
    }

    private void StopSlide(DestType dest)
    {
        if (dest != DestType.DOOR_OUT)
            return;
        StopAllCoroutines();
        _player.Stop();
    }

    private IEnumerator SwitchSlide_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);
            _player.NextSlide();
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Z))
    //        _player.Play();
    //    if (Input.GetKeyDown(KeyCode.X))
    //        _player.Stop();
    //    if (Input.GetKeyDown(KeyCode.C))
    //        _player.NextSlide();
    //    if (Input.GetKeyDown(KeyCode.V))
    //        _player.PrevSlide();
    //    if (Input.GetKeyDown(KeyCode.B))
    //        _player.LoadSlides("DefaultSlides");
    //}
}
