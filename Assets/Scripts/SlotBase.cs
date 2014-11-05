using UnityEngine;
using System.Collections;

namespace EveryStudio.SlotGame
{
	public class SlotBase : MonoBehaviour {

		public const int REEL_NUM = 3;

		public GameObject [] m_prefReel = new GameObject[REEL_NUM];

		public int [] m_iReelOrder = {0,2,1};

		public SlotReelBase [] m_tReelBase;

		public float m_fSymbolWidth;
		public float m_fSymbolHeight;

		public void Init(){

			m_fSymbolWidth = 2.0f;
			m_fSymbolHeight= 2.0f;

			m_tReelBase = new SlotReelBase[REEL_NUM];

			for( int i = 0 ; i < REEL_NUM ; i++){
				GameObject obj = Instantiate (m_prefReel[i], transform.localPosition,  transform.rotation) as GameObject;
				obj.name = "reel" + i.ToString("00");
				obj.transform.parent = transform;

				int iOrder = m_iReelOrder[i];

				obj.transform.localPosition = new Vector3( iOrder*m_fSymbolWidth , 0.0f , 0.0f );

				m_tReelBase[i] = obj.GetComponent<SlotReelBase>();
				m_tReelBase[i].Init( m_fSymbolWidth , m_fSymbolHeight );
				//コンストラクタで初期化するので
				//m_tReelBase[i].Init();
			}
		}

		public void StartSpin(){

			SafetyLogger.Instance.Write( "test_log" );

			CommonNetwork.Instance.request(E_NETWORK.TEST);

			for( int i = 0 ; i < REEL_NUM ; i++ ){
				float fDelay = 0.3f;
				m_tReelBase[i].SpinStart( fDelay * i );
			}
		}

		public void StopStartRandom(){
			for( int i = 0 ; i < REEL_NUM ; i++ ){
				float fDelay = 0.3f;
				m_tReelBase[i].StopStartRandom( fDelay * i );
			}
		}

	}
}
/*

http://staging-puzzle-kindom-853004120.ap-northeast-1.elb.amazonaws.com/start{"dummy":0}
http://staging-puzzle-kindom-853004120.ap-northeast-1.elb.amazonaws.com/start{"uuid":"2d4b588b-8f41-42b8-a255-e0554d80a3b5","uid":271}

UnityEngine.Debug:Log(Object)
UnityEngine.Debug:Log(Object)
<send_module>c__Iterator6:Move
*/







