using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//스테이지 연출과 스코어 평가 유닛 등, 음악에 맞추어 실행되는 처리의 기본 클래스
public abstract class MusicalElement {
	//처리를 시작할 수 있는 타이밍.        
	public float triggerBeatTiming = 0;
	//파라미터값의 문자열 배열(CSV등의 읽기에 사용) 
	public virtual void ReadCustomParameterFromString(string[] parameters){}
	//triggerBeatTimingの原点を指定した上でクローンを生成
	public virtual MusicalElement GetClone(){
		MusicalElement clone = this.MemberwiseClone() as MusicalElement;
		return clone;
	}
	public System.Xml.Schema.XmlSchema GetSchema(){return null;}
};
//음악 파트(A메로、사비등)정보를 저장하는 클래스
public class SequenceRegion: MusicalElement{
	public float totalBeatCount;
	public string name;
	public float repeatPosition;
};

//플레이어가 음악에 맞추어 샐행해야 하는 액션 정보
public class OnBeatActionInfo : MusicalElement {
	public PlayerActionEnum playerActionType;//액션의 종류
    public string GetCustomParameterAsString_CSV(){
		return "SingleShot," + triggerBeatTiming.ToString() + "," + playerActionType.ToString();
	}

	public int	line_number;		// 텍스트 내의 행 번호.
}
