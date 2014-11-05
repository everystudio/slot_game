using UnityEngine;
using System.Collections;
using System;

#pragma warning disable 0414

namespace EveryStudio.SlotGame
{
	public class SlotReelBase : MonoBehaviour {

		public int m_iNumber;
		public GameObject m_prefSymbol;

		[SerializeField]
		public int [] m_iSymbolArr;

		protected const int SYMBOL_DISP_NUM = 4;

		[SerializeField]
		protected SlotSymbolBase [] m_tSymbolArr = new SlotSymbolBase[SYMBOL_DISP_NUM];

		[SerializeField]
		protected TSymbolData [] m_tSymbolDataArr;

		public int m_iSymbolIndex;
		public int m_iTargetIndex;
		public float m_fMoveTime;
		public float m_fReelSpeed;
		public float m_fDelayTime;
		public float m_fSymbolInterval;
		public float m_fSymbolWidth;
		public float m_fSymbolHeight;

		public enum  STEP {
			NONE		= 0,
			IDLE		,
			SPIN_START	,
			LOOP		,
			STOP_DELAY	,
			STOP_START	,
			STOPPED		,

			MAX			,
		}
		public STEP m_eStep;
		public STEP m_eStepPre;
		public float m_fTimer;

		public void Init( float _fWidth , float _fHeight ){
			// シンボルのサイゾ設定
			SetSymbolSize(_fWidth , _fHeight );

			m_eStep = STEP.IDLE;
			m_eStepPre = STEP.MAX;
			m_iSymbolIndex = 0;
			m_fMoveTime = 0.0f;
			m_fReelSpeed = 0.0f;
			m_fDelayTime = 0.0f;

			for( int i = 0; i < SYMBOL_DISP_NUM ; i++ ){
				GameObject obj = Instantiate (m_prefSymbol, transform.localPosition,  transform.rotation) as GameObject;
				obj.name = "symbol" + i.ToString("00");
				obj.transform.parent = transform;
				m_tSymbolArr[i] = obj.GetComponent<SlotSymbolBase>();
				m_tSymbolArr[i].Init( m_fSymbolWidth , m_fSymbolHeight );
			}
			Disp( m_iSymbolIndex , m_fMoveTime);
		}

		protected void Disp( int _iIndex , float _fTime ){

			if( _iIndex < 0 ){
				_iIndex = 0;
			}
			else if( m_tSymbolDataArr.Length <= _iIndex ){
				_iIndex = 0;
			}
			else {
				;// ok
			}

			float fOffset = -1.0f * m_fMoveTime * (m_fSymbolHeight + m_fSymbolInterval);
			for( int i = 0 ; i < SYMBOL_DISP_NUM ; i++ ){

				int iSymbolDataIndex = _iIndex + i;
				iSymbolDataIndex %= m_tSymbolDataArr.Length;

				Sprite sprImage = m_tSymbolDataArr[iSymbolDataIndex].sprSprite;
				m_tSymbolArr[i].SetPos( 0.0f , i*m_fSymbolInterval + fOffset);
				m_tSymbolArr[i].SetSprite( sprImage );
			}
		}

		public int ProgressSymbol( int _iAddIndex ){
			m_iSymbolIndex += _iAddIndex;
			m_iSymbolIndex %= m_tSymbolDataArr.Length;
			return m_iSymbolIndex;
		}

		public int ProgressReel( float _fDeltaTime , bool _bIndexIncriment){
			m_fMoveTime += _fDeltaTime;
			double dFloor = Math.Floor(m_fMoveTime);

			m_fMoveTime -= (float)dFloor;

			int iAddIndex = (int)dFloor;

			if( _bIndexIncriment ){
				if( 0 < dFloor ){
					m_iSymbolIndex = ProgressSymbol(iAddIndex);
				}
			}

			Disp( m_iSymbolIndex , m_fMoveTime );
			return iAddIndex;
		}

		public void exe(){

			bool bInit = false;
			if( m_eStepPre != m_eStep ){
				m_eStepPre  = m_eStep;
				bInit = true;
			}

			switch( m_eStep )
			{
			case STEP.NONE:
				break;
			case STEP.IDLE:
				if( bInit) {
					;// 一応位置調整しておきましょうか？
					m_fReelSpeed = 0.0f;
					Disp( m_iSymbolIndex , m_fMoveTime );
				}
				break;

			case STEP.SPIN_START:
				if( bInit ){
					m_fTimer = 0.0f;
				}
				m_fTimer += Time.deltaTime;
				if( m_fDelayTime < m_fTimer ){
					m_eStep = STEP.LOOP;
				}
				break;
			case STEP.LOOP:
				if( bInit ){
					m_fReelSpeed = 25.0f;
				}
				ProgressReel(Time.deltaTime*m_fReelSpeed,true);
				break;

			case STEP.STOP_DELAY:
				if( bInit ){
					m_fTimer = 0.0f;
				}
				ProgressReel(Time.deltaTime*m_fReelSpeed,true);
				m_fTimer += Time.deltaTime;
				if( m_fDelayTime < m_fTimer ){
					m_eStep = STEP.STOP_START;
				}
				break;

			case STEP.STOP_START:
				if( bInit ){
					m_iSymbolIndex = GetSymbolIndex( m_iTargetIndex , -3 );
				}
				int iAddIndex = ProgressReel(Time.deltaTime*m_fReelSpeed,false);
				if( 0 < iAddIndex ){
					ProgressSymbol(iAddIndex);
					if( m_iSymbolIndex==m_iTargetIndex){
						m_eStep = STEP.STOPPED;
						m_fMoveTime = 0.0f;
					}
					Disp(m_iSymbolIndex,m_fMoveTime);
				}

				break;
			case STEP.STOPPED:
				if( bInit ){
					//Debug.Log(m_iSymbolIndex);
					m_eStep = STEP.IDLE;
				}
				break;
			default:
				break;
			}
		}

		public void SpinStart( float _fDelay ){
			if( m_eStep == STEP.IDLE ){
				m_fDelayTime = _fDelay;
				m_fReelSpeed = 3.0f;
				m_eStep = STEP.SPIN_START;
			}
			return;
		}

		public void StopStartRandom(float _fDelay){
			if( m_eStep == STEP.LOOP ){
				m_eStep = STEP.STOP_DELAY;
				m_fDelayTime = _fDelay;

				/*
				// Random クラスの新しいインスタンスを生成する
				Random random = new System.Random();

				// 0 以上 512 未満の乱数を取得する
				int iResult2 = random.Next(m_tSymbolDataArr.Length);
				*/
				int iResult2 = UnityEngine.Random.Range(0,m_tSymbolDataArr.Length);
				m_iTargetIndex = iResult2;
			}
		}

		public void StopStart( int _iIndex ){
			return;
		}

		public int GetSymbolIndex( int _iIndex , int _iOffset ){

			while( _iIndex < 0 ){
				_iIndex += m_tSymbolDataArr.Length;
			}

			_iIndex += _iOffset;

			return _iIndex % m_tSymbolDataArr.Length;
		}

		public void SetSymbolSize(float _fWidth , float _fHeight ){
			m_fSymbolWidth = _fWidth;
			m_fSymbolHeight= _fHeight;
			return;
		}
	}
}














