/// </summary>
//sound관리하는 manager임
//Singleton 사용함 [DOTWEEN,ODIN]
//Resource Path설정
//Bgm 은 그냥 넣으면됨
//Ef 는 이름이 key 이고 파일이름 마지막에 몇개 설정할지 넣으면됨
// Ef
//     SoundPlay_Ef :  ef 재생 , 볼륨 루핑 설정가능
//     SoundPlay_Ef_Decrease : ef 중첩소리 작아지게 재생 , 초기소리 점점 작아지는정도 [0.9(10%씩작아짐)] , 최소값 설정가능
// Bgm
//     SoundPlay_Bgm_Direct : 바로재생
//     SoundPlay_Bgm_Increaas : 점점 커지면서 재생 , 시작볼륨, 끝볼륨 , 끝까지 올라가는 시간 조절가능
// Stop
//     StopAllSource_Direct : 모든소리 즉시 정지
//     StopBgm_Direct : 배경음만 즉시정지
//     StopEf_Direct : 해당 이팩트 즉시정지
//     StopBgm_Decrease : 배경음 점점 정지 , 몇초동안
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using DG.Tweening;

public class SoundManager : SINGLETON<SoundManager,SINGLETONE.SINGLETONEType.DontDestroy>
{
    [SerializeField] private string ReSourcePath_Bgm="";
    [SerializeField] private string ReSourcePath_Ef="";
    //Bgm
    [SerializeField] private AudioSource AudioSource_Bgm;
    [SerializeField] private Dictionary<string, AudioClip> Dic_BgmAudioClip_NameToList=new Dictionary<string, AudioClip>();
    //Ef
    [SerializeField] private Dictionary<string, List<AudioSource>> Dic_EfListAudioSource_NameToList=new Dictionary<string, List<AudioSource>>();
    
    
    public List<AudioSource> ListAudioSource_All;



    protected override void Awake()
     {
         base.Awake();//안꺼지게하는거임
                 
         //init bgm
        //audiosource생성
        GameObject goAudioSourceBgm = new GameObject("AudioSrouce_Bgm");
        goAudioSourceBgm.transform.parent = gameObject.transform;
        AudioSource_Bgm= goAudioSourceBgm.AddComponent<AudioSource>();
        AudioSource_Bgm.loop = true;
        ListAudioSource_All.Add(AudioSource_Bgm);
        
        //audoiclip 생성
        AudioClip[] arrayAudiosourceBgm = Resources.LoadAll<AudioClip>(ReSourcePath_Bgm);
        for (int i = 0; i < arrayAudiosourceBgm.Length; i++)
            Dic_BgmAudioClip_NameToList.Add(string.Concat(arrayAudiosourceBgm[i].name.Where(c => !char.IsWhiteSpace(c))), arrayAudiosourceBgm[i]);//문자열의 공백을 지움 ex) "A bcd"=>"Abcd"



        //init ef
        AudioClip[] arrayAudioClipEf = Resources.LoadAll<AudioClip>(ReSourcePath_Ef);
        for (int i = 0; i < arrayAudioClipEf.Length; i++)
        {
            //이름,갯수설정
            var clipname = arrayAudioClipEf[i].name.Substring(0, arrayAudioClipEf[i].name.Length - 1);
            if(!int.TryParse(arrayAudioClipEf[i].name.Substring(arrayAudioClipEf[i].name.Length - 1),out int count)) //file 이름 마지막이 숫자가아님
                Debug.LogError("SoundManager init error "+clipname+"Ef LastName is Not Int");

                if (count == 0) //count 가 0임
                Debug.LogError("SoundManager init error "+clipname+"count 0");
            if (Dic_EfListAudioSource_NameToList.ContainsKey(clipname)) //이름겹침
                    Debug.LogError("SoundManager init error "+clipname+"already in dic");

                //오브젝트만들고
            GameObject goAudoiSourceBgm =
                new GameObject(clipname);
            goAudoiSourceBgm.transform.parent = gameObject.transform;
            
            //리스트에 집어넣음
            List<AudioSource> listaudiosource = new List<AudioSource>();
            for (int j = 0; j < count; j++)
            {
                AudioSource audio = goAudoiSourceBgm.AddComponent<AudioSource>();
                audio.clip = arrayAudioClipEf[i];
                audio.loop = false;
                audio.playOnAwake = false;
                listaudiosource.Add(audio);
                ListAudioSource_All.Add(audio);
            }
            Dic_EfListAudioSource_NameToList.Add(clipname,listaudiosource);
        }
     }


    /// <summary>
    /// bgm 재생 바로
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolum"></param> 시작볼륨
    #region  bgm
    public void SoundPlay_Bgm_Direct(string soundName, float startVolum)
    {
        if(!Dic_BgmAudioClip_NameToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        AudioSource_Bgm.Stop();
        AudioSource_Bgm.clip = Dic_BgmAudioClip_NameToList[soundName];
        AudioSource_Bgm.volume = startVolum;
        AudioSource_Bgm.Play();
    } 
    /// <summary>
    /// bgm 재생 점점 커지게
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolum"></param> 시작볼륨
    /// <param name="lastVolum"></param> 끝날때볼륨
    /// <param name="duringTime"></param> 몇초동안?
    public void SoundPlay_Bgm_Increaas(string soundName, float startVolum, float lastVolum, float duringTime)
    {
        if(!Dic_BgmAudioClip_NameToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        if(lastVolum<=startVolum)
            Debug.LogError("SoundManagerError - bgm increase 이럼 소리가 꺼지거나 direct 써야함"); 
        AudioSource_Bgm.Stop();
        AudioSource_Bgm.clip = Dic_BgmAudioClip_NameToList[soundName];
        AudioSource_Bgm.volume = startVolum;
        AudioSource_Bgm.Play();
        AudioSource_Bgm.DOFade(lastVolum, duringTime);
    } //
    

    #endregion

    #region Ef
    /// <summary>
    /// ef 재생
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolume"></param> 시작볼륨
    /// <param name="loop"></param> 반복할꺼임?
    public void SoundPlay_Ef(string soundName, float startVolume = 1, bool loop=false)
    {
        Debug.Log(soundName);
        AudioSource audioSource = Dic_ReturnAudioClip_Ef(soundName);
        if (audioSource == null)
            return;
        audioSource.volume = startVolume;
        audioSource.Play();
    } 
    /// <summary>
    /// ef 재생중인 소리가 많을수록 중첩될수록 점점 소리가 작아짐
    /// </summary>
    /// <param name="soundName"></param> 사운드이름
    /// <param name="startVolum"></param> 시작볼륨
    /// <param name="decraseVolume"></param> 중복시 작아지는 값 ex) 0.9f ,0.8f
    /// <param name="minvolume"></param> 최소볼륨
    public void SoundPlay_Ef_Decrease(string soundName, float startVolum, float decraseVolume, float minvolume) 
    {
        AudioSource audioSource = Dic_ReturnAudioClip_Ef(soundName);
        if (audioSource == null)
            return;
        
        int a = ReturnPlay_AudiosourceCount_Ef(soundName);
        float finalvalum =Mathf.Clamp(startVolum * math.pow(decraseVolume, a),minvolume,1);
        for (int i = 0; i < Dic_EfListAudioSource_NameToList[soundName].Count; i++)
        {
            Dic_EfListAudioSource_NameToList[soundName][i].volume = finalvalum;
        }
        audioSource.Play();
    }

    #endregion

    #region 정지
    /// <summary>
    /// 모두다정지 즉시
    /// </summary>
    public void StopAllSource_Direct() 
    {
        for(int i=0;i<ListAudioSource_All.Count;i++)
            ListAudioSource_All[i].Stop();
    }
    /// <summary>
    /// 배경음 즉시정지
    /// </summary>
    public void StopBgm_Direct() 
    {
        AudioSource_Bgm.Stop();
    }
    /// <summary>
    /// 모두다정지 천천히
    /// </summary>
    /// <param name="during"></param> 몇초동안
    /// <param name="after"></param> 다음동작
    public void StopAllSource_Increase(float during,Action after=null) 
    {
        for (int i = 0; i < ListAudioSource_All.Count; i++)
            DoFadeAndStop(ListAudioSource_All[i], during,after);
    }


    /// <summary>
    /// 해당 이팩트 정지
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    public void StopEf_Direct(string soundName) 
    {
        if(!Dic_EfListAudioSource_NameToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        
        for(int i=0;i<Dic_EfListAudioSource_NameToList[soundName].Count;i++)
            Dic_EfListAudioSource_NameToList[soundName][i].Stop();
    }


    /// <summary>
    /// 배경음 점점정지
    /// </summary>
    /// <param name="druing"></param> 몇초동안?
    /// <param name="after"></param> 다음동작
    public void StopBgm_Decrease(float druing,Action after) //
    {
        DoFadeAndStop(AudioSource_Bgm, druing, after);
    }
    

    #endregion

    #region Etc
    /// <summary>
    /// ef  dic 에서 재생중이지 않은 트랙 리턴
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <returns></returns>
    AudioSource Dic_ReturnAudioClip_Ef(string soundName) 
    {
        if(!Dic_EfListAudioSource_NameToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);

        for (int i = 0; i < Dic_EfListAudioSource_NameToList[soundName].Count; i++)
        {
            if (!Dic_EfListAudioSource_NameToList[soundName][i].isPlaying)
                return Dic_EfListAudioSource_NameToList[soundName][i];
        }

        return null; //전부다 실행 중이라는 뜻임 무시하고 넘어감
    }//오디오찾기
    /// <summary>
    /// ef 중 몇개가 재생중인가 확인
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <returns></returns>
    int ReturnPlay_AudiosourceCount_Ef(string soundName) //
    {
        int a = 0;
        if(!Dic_EfListAudioSource_NameToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        for (int i = 0; i < Dic_EfListAudioSource_NameToList[soundName].Count; i++)
        {
            if (Dic_EfListAudioSource_NameToList[soundName][i].isPlaying)
                a++;
        }

        return a;
    }
    /// <summary>
    /// 닷트윈 확장 메서드 소리점점 작아지고 이후 멈추게 
    /// </summary>
    /// <param name="targetAudioSource"></param>
    /// <param name="durTime"></param>
    /// <param name="after"></param>
    void DoFadeAndStop(AudioSource targetAudioSource, float durTime,Action after=null)
    {
        Sequence Seq_FadeAndStop = DOTween.Sequence()
            .Append(targetAudioSource.DOFade(0, durTime))
            .AppendCallback(() =>
            {
                targetAudioSource.Stop();
                after?.Invoke();
            });
    }
    

        #endregion

    
}







