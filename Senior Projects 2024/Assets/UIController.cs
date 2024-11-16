using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public Slider _musicSlider, _sfxSlider;
    public void ToggleMusic()
    {
        AudioMgr.Instance.ToggleMusic();
    }
 
    public void ToggleSFX()
    {
        AudioMgr.Instance.ToggleSFX();
    }
    public void MusicVolume()
    {
        AudioMgr.Instance.MusicVolume(_musicSlider.value);
    }

    public void sfxVolume()
    {
        AudioMgr.Instance.sfxVolume(_sfxSlider.value);
    }
}
