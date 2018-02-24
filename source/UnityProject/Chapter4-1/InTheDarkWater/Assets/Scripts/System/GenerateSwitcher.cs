using UnityEngine;
using System.Collections;

/// <summary>
/// 생성물 변경. 현재 RandomGenerator에 의해 생성되는것만 고려한다.
/// </summary>
public class GenerateSwitcher : MonoBehaviour {

    enum Type
    {
        None = 0,
        OnlyOne,
        Switch,
//        Random,
//        All,
    };
    [SerializeField]
    private Type type = Type.None;
    [SerializeField]
    private string currentTag = "Enemey";

    public class TargetGenerator
    {
        public bool clearCondition = false;
        public RandomGenerator gen = null;
    };

    private TargetGenerator current = null;
    private Hashtable generators = new Hashtable();

	void Start () 
    {
    }

    private void Init()
    {
        RandomGenerator[] genArr = GetComponentsInChildren<RandomGenerator>();
        foreach( RandomGenerator gen in genArr ){
            AddGenerator(gen);
        } 
    }

    private void AddGenerator( RandomGenerator generater )
    {
        Debug.Log("AddGenerator");
        if (generater == null) return;

        GameObject target = generater.Target();
        Debug.Log("Target:" + target.tag);
        if (target == null) return;
        string tag = target.tag;

        TargetGenerator targetGenerator = new TargetGenerator();
        targetGenerator.clearCondition = false;
        targetGenerator.gen = generater;
        generators.Add(tag, targetGenerator);
    }
	

    void OnSwitchCheck( string key )
    {
		Debug.Log("OnSwitchCheck:" + key);
        if (currentTag.Equals(key))
        {
            switch (type)
            {
                case Type.Switch: Switch(); break;
                // 패턴을 변경하고 싶은 경우에 추가.
                //case Type.Random: SetRandom(); break;
                default: break;
            }
        }
    }

    private void Run()
    {
        if (!generators.ContainsKey(currentTag))
        {
            Debug.Log(currentTag + ": Not Exist!");
            return;
        }
        current = generators[currentTag] as TargetGenerator;
        current.gen.SendMessage("OnGeneratorStart");
    }

    /// <summary>
    /// Switch한다.
    /// </summary>
    private void Switch()
    {
        if (generators.Count == 0) return;

        Suspend();
        //current.gen.SendMessage("OnGeneratorSuspend");

        foreach( string key in generators.Keys )
        {
            if (!currentTag.Equals(key))
            {
                currentTag = key;
                Run();
                return;
            }
        }
    }

    // 중단
    private void Suspend()
    {
        if (current == null) return;
        current.gen.SendMessage("OnGeneratorSuspend");
    }


    // 게임시작 알림
    void OnGameStart()
    {
        Init();
        Run();
    }
    // 게임 종료 알림
    void OnGameOver()
    {
        Suspend();
    }
    // 게임 클리어 알림
    void OnGameClear()
    {
        Suspend();
    }


    void OnClearCondition(string tag)
    {
        // 클리어 조건
        bool allClear = true;
        foreach (string key in generators.Keys) 
        {
            // 게임을 달성한 태그의 TargetObject를 true로 한다.
            TargetGenerator target = generators[key] as TargetGenerator;
            if (tag.CompareTo(key) == 0) target.clearCondition = true;
            // 전부 클리어하였는지 체크
            allClear &= target.clearCondition;
        }

        if (allClear) {
            // 게임 종류, 다음 스테이지로.
            GameObject adapter = GameObject.Find("/Adapter");
            adapter.SendMessage("OnGameEnd", true);
        }
    }
}
