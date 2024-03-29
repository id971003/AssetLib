/*
Made 7rzr 2023-01-2
UPDATE 7rzr 2023-12-14

조건 : odin , singleton , Dotween [의존성추가]
세팅  [Resource 폴더 안에 넣어야 받아올수있따]
    ReSourcePath_Bgm[bgm 파일들 경로]
    ReSourcePath_Ef[ef 파일들 경로]
    filePath[ENUM타입 저장할위치]
    
sound 관리하는 manager 임  
resource사운드파일들 이름들을 ENUM 타입으로 만들어서 ENUM으로 타입으로 사운드 콜가능
EF 들의 경우 소리_몇개 설정할지 해줘야함 즉 이름 마지막에 _"몇개설정할지" 해주면됨    

INIT_ENUM : RESOURCE 불러와서 ENUM 타입만듬
INIT_Sound : Reousrce 기준으로 Enum 타입에 해당하는 AudioSource 만들고 Dic 에 정리함 


play
     SoundPlay_Bgm_Direct : bgm 즉시재생
     SoundPlay_Bgm_Increaas_DoTween : bgm 점점커지면서 재생
     
     SoundPlay_Ef : ef 재생
     SoundPlay_Ef_Decrease : ef 재생하는데 이미 동일한 타입 재생중이면 소리 작게 재생
     
Stop
    StopBgm_Direct : bgm 즉시 정지
    StopBgm_Decrease_DoTween : bgm 점점 정지
    
    StopEf_Direct : ef 해당타입 모두 정지
    
    StopAllSource_Direct : 모든 소리 즉시 정지
    StopAllSource_Decrease_DoTween : 모든소리 점점 작아지면서 정지


 */
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Sirenix.OdinInspector;
using System.IO;
using SoundManager_Enum;
using DG.Tweening;

public class SoundManager : SINGLETON<SoundManager,Ns_SINGLETONE.SINGLETONEType.DontDestroy>
{
    private string filePath=@"Assets\Scripte\Lib\SoundManager\SoundManager_Enum.cs";
    private readonly string message = "namespace SoundManager_Enum { public enum soundManager_Enum {";
    private readonly string message2 = "}}\n";
    private string ReSourcePath_Bgm=@"Sound\Bgm";
    private string ReSourcePath_Ef=@"Sound/Ef";
    //Bgm
    [SerializeField] private AudioSource AudioSource_Bgm;
    [SerializeField] private Dictionary<soundManager_Enum, AudioClip> Dic_BgmAudioClip_EnumToList=new Dictionary<soundManager_Enum, AudioClip>();
    //Ef
    [SerializeField] private Dictionary<soundManager_Enum, List<AudioSource>> Dic_EfListAudioSource_EnumToList=new Dictionary<soundManager_Enum, List<AudioSource>>();
    
    
    public List<AudioSource> ListAudioSource_All;


    private AudioClip[] ppoArrayBgm;
    private AudioClip[] ppoArrayEf;
    
    /// <summary>
    /// soundname 으로 enum 타입 만듬
    /// </summary>
    [Button]
    void INIT_ENUM()
    {
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);

        writer.WriteLine(message);
        //init bgm
        ppoArrayBgm=Resources.LoadAll<AudioClip>(ReSourcePath_Bgm);
        for (int i = 0; i < ppoArrayBgm.Length; i++)
        {
            //enum 생성
            if (i != 0)
            {
                writer.WriteLine(","+ ppoArrayBgm[i].name);
            }
            else
            {
                writer.WriteLine(ppoArrayBgm[i].name);    
            }
            
        }
        //init ef
        ppoArrayEf = Resources.LoadAll<AudioClip>(ReSourcePath_Ef);
        for (int i = 0; i < ppoArrayEf.Length; i++)
        {
            if (ppoArrayBgm.Length == 0 && i==0)
            {
                int countslid = ppoArrayEf[i].name.LastIndexOf('_');
                writer.WriteLine(ppoArrayEf[i].name.Substring(0,countslid));
            }
            else
            {
                int countslid = ppoArrayEf[i].name.LastIndexOf('_');
                writer.WriteLine(","+ ppoArrayEf[i].name.Substring(0,countslid));   
            }
            
        }


        writer.WriteLine(message2);
        writer.Close();
        fileStream.Close();
    }
    
    /// <summary>
    /// 만든enum타입 기준으로 사운드트랙 집어넣음
    /// </summary>
    [Button]
    void INIT_Sound()
    {
       for (int i = transform.childCount-1; i >=0 ; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Dic_BgmAudioClip_EnumToList.Clear();
        Dic_EfListAudioSource_EnumToList.Clear();
        ListAudioSource_All.Clear();

        
        GameObject goAudioSourceBgm = new GameObject("AudioSrouce_Bgm");
        goAudioSourceBgm.transform.parent = gameObject.transform;
        AudioSource_Bgm= goAudioSourceBgm.AddComponent<AudioSource>();
        AudioSource_Bgm.loop = true;
        ListAudioSource_All.Add(AudioSource_Bgm);
        
        //audoiclip 생성
        for (int i = 0; i < ppoArrayBgm.Length; i++)
        {
            Dic_BgmAudioClip_EnumToList.Add(
                (soundManager_Enum)Enum.Parse(typeof(soundManager_Enum),string.Concat(ppoArrayBgm[i].name.Where(c => !char.IsWhiteSpace(c)))),
                ppoArrayBgm[i]); //문자열의 공백을 지움 ex) "A bcd"=>"Abcd"
        }
        

        //init ef
        for (int i = 0; i < ppoArrayEf.Length; i++)
        {
            //이름,갯수설정
            int countslide = ppoArrayEf[i].name.LastIndexOf('_');
            string clipname = ppoArrayEf[i].name.Substring(0, countslide);
            soundManager_Enum clipname_enum = (soundManager_Enum)Enum.Parse(typeof(soundManager_Enum),
                string.Concat(clipname.Where(c => !char.IsWhiteSpace(c))));
            if(!int.TryParse(ppoArrayEf[i].name.Substring(countslide+1),out int count)) //file 이름 마지막이 숫자가아님
                Debug.LogError("SoundManager init error "+clipname+"Ef LastName is Not Int");

                if (count == 0) //count 가 0임
                Debug.LogError("SoundManager init error "+clipname+"count 0");
            if (Dic_EfListAudioSource_EnumToList.ContainsKey(clipname_enum)) //이름겹침
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
                audio.clip = ppoArrayEf[i];
                audio.loop = false;
                audio.playOnAwake = false;
                listaudiosource.Add(audio);
                ListAudioSource_All.Add(audio);
            }
            Dic_EfListAudioSource_EnumToList.Add(clipname_enum,listaudiosource);
        }
    }





    #region  Play
    /// <summary>
    /// bgm 재생 바로
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolum"></param> 시작볼륨
    public void SoundPlay_Bgm_Direct(soundManager_Enum soundName, float startVolum)
    {
        if(!Dic_BgmAudioClip_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        AudioSource_Bgm.Stop();
        AudioSource_Bgm.clip = Dic_BgmAudioClip_EnumToList[soundName];
        AudioSource_Bgm.volume = startVolum;
        AudioSource_Bgm.Play();
    }
    
    /// <summary>
    /// bgm 점점 커지면서 재생
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolum"></param> 시작볼륨
    /// <param name="lastVolum"></param> 마지막볼륨
    /// <param name="duringTime"></param> 커질시간
    public void SoundPlay_Bgm_Increaas_DoTween(soundManager_Enum soundName, float startVolum, float lastVolum, float duringTime)
    {
        if(!Dic_BgmAudioClip_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        if(lastVolum<=startVolum)
            Debug.LogError("SoundManagerError - bgm increase 이럼 소리가 꺼지거나 direct 써야함"); 
        AudioSource_Bgm.Stop();
        AudioSource_Bgm.clip = Dic_BgmAudioClip_EnumToList[soundName];
        AudioSource_Bgm.volume = startVolum;
        AudioSource_Bgm.Play();
        AudioSource_Bgm.DOFade(lastVolum, duringTime);
    } 
    
    
    /// <summary>
    /// ef 재생
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolume"></param> 시작볼륨
    /// <param name="loop"></param> 반복할꺼임?
    public void SoundPlay_Ef(soundManager_Enum soundName, float startVolume = 1, bool loop=false)
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
    public void SoundPlay_Ef_Decrease(soundManager_Enum soundName, float startVolum, float decraseVolume, float minvolume) 
    {
        AudioSource audioSource = Dic_ReturnAudioClip_Ef(soundName);
        if (audioSource == null)
            return;
        
        int a = ReturnPlay_AudiosourceCount_Ef(soundName);
        float finalvalum =Mathf.Clamp(startVolum * math.pow(decraseVolume, a),minvolume,1);
        for (int i = 0; i < Dic_EfListAudioSource_EnumToList[soundName].Count; i++)
        {
            Dic_EfListAudioSource_EnumToList[soundName][i].volume = finalvalum;
        }
        audioSource.Play();
    }
    
    #endregion

    #region 정지
    
    /// <summary>
    /// 배경음 즉시정지
    /// </summary>
    public void StopBgm_Direct() 
    {
        AudioSource_Bgm.Stop();
    }
    
    /// <summary>
    /// 배경음 점점정지
    /// </summary>
    /// <param name="druing"></param> 몇초동안?
    /// <param name="after"></param> 다음동작
    public void StopBgm_Decrease_DoTween(float druing,Action after) //
    {
        DoFadeAndStop(AudioSource_Bgm, druing, after);
    
    }
    
    /// <summary>
    /// 모두다정지 즉시
    /// </summary>
    public void StopAllSource_Direct() 
    {
        for(int i=0;i<ListAudioSource_All.Count;i++)
            ListAudioSource_All[i].Stop();
    }
    
    /// <summary>
    /// 해당 이팩트 정지
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    public void StopEf_Direct(soundManager_Enum soundName) 
    {
        if(!Dic_EfListAudioSource_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        
        for(int i=0;i<Dic_EfListAudioSource_EnumToList[soundName].Count;i++)
            Dic_EfListAudioSource_EnumToList[soundName][i].Stop();
    }

    /// <summary>
    /// 모두다정지 천천히
    /// </summary>
    /// <param name="during"></param> 몇초동안
    /// <param name="after"></param> 다음동작
    public void StopAllSource_Decrease_DoTween(float during,Action after=null) 
    {
        for (int i = 0; i < ListAudioSource_All.Count; i++)
            DoFadeAndStop(ListAudioSource_All[i], during,after);
    }
    
    #endregion

    #region Etc

    /// <summary>
    /// ef  dic 에서 재생중이지 않은 트랙 리턴
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <returns></returns>
    AudioSource Dic_ReturnAudioClip_Ef(soundManager_Enum soundName)
    {
        if (!Dic_EfListAudioSource_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음" + soundName);

        for (int i = 0; i < Dic_EfListAudioSource_EnumToList[soundName].Count; i++)
        {
            if (!Dic_EfListAudioSource_EnumToList[soundName][i].isPlaying)
                return Dic_EfListAudioSource_EnumToList[soundName][i];
        }

        return null; //전부다 실행 중이라는 뜻임 무시하고 넘어감
    } //오디오찾기

    /// <summary>
    /// ef 중 몇개가 재생중인가 확인
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <returns></returns>
    int ReturnPlay_AudiosourceCount_Ef(soundManager_Enum soundName) //
    {
        int a = 0;
        if (!Dic_EfListAudioSource_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음" + soundName);
        for (int i = 0; i < Dic_EfListAudioSource_EnumToList[soundName].Count; i++)
        {
            if (Dic_EfListAudioSource_EnumToList[soundName][i].isPlaying)
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
    void DoFadeAndStop(AudioSource targetAudioSource, float durTime, Action after = null)
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

    #region OldCode

    /*
     
     //    SoundAdjest_InTime(AudioSource_Bgm, AudioSource_Bgm.volume ,druing, 0.1f, after);
     *     /// <summary>
    /// bgm 재생 점점 커지게
    /// </summary>
    /// <param name="soundName"></param> 트랙이름
    /// <param name="startVolum"></param> 시작볼륨
    /// <param name="lastVolum"></param> 끝날때볼륨
    /// <param name="duringTime"></param> 몇초동안?
    public void SoundPlay_Bgm_Increaas(soundManager_Enum soundName, float startVolum, float lastVolum, float duringTime,float deltaTIme=0.1f)
    {
        if(!Dic_BgmAudioClip_EnumToList.ContainsKey(soundName))
            Debug.LogError("SoundManagerError - key 없음"+soundName);
        if(lastVolum<=startVolum)
            Debug.LogError("SoundManagerError - bgm increase 이럼 소리가 꺼지거나 direct 써야함"); 
        AudioSource_Bgm.Stop();
        AudioSource_Bgm.clip = Dic_BgmAudioClip_EnumToList[soundName];
        AudioSource_Bgm.volume = startVolum;
        AudioSource_Bgm.Play();
        SoundAdjest_InTime(AudioSource_Bgm, lastVolum, duringTime, deltaTIme);
    } //
    
     void SoundAdjest_InTime(AudioSource Target, float EndSound, float During,float DecreasTime=0.1f,Action AfterProcess=null)
    {
        StartCoroutine(C_SoundAdjest_InTime(Target, EndSound, During, DecreasTime,AfterProcess));
    }

    IEnumerator C_SoundAdjest_InTime(AudioSource Target, float EndSound, float During,float DecreasTime=0.1f,Action AfterProcess=null)
    {
        WaitForSeconds DeltaTIme = new WaitForSeconds(DecreasTime);
        float DeltaVolum=(Target.volume - EndSound) / During * DecreasTime;
        while (true)
        {
            Target.volume -= DeltaVolum;
            yield return DeltaTIme;
            if (Target.volume <= DeltaVolum)
            {
                Target.volume = DeltaVolum;
                AfterProcess?.Invoke();
                break;
            }
        }
    }

     */

    #endregion

    protected override void Awake()
    {
        base.Awake(); //안꺼지게하는거임

        //init bgm
        //audiosource생성
        INIT_ENUM();
        INIT_Sound();
    }
}
