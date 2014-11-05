using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public enum E_NETWORK{
	TEST			,		// テスト用
	LIMIT						//=256
}

public class CommonNetwork : MonoBehaviour {

	const int TIMEOUT_TIME 				= 20;
	const int SESSION_ERROR_CODE 		=  9;
	const int MAINTENANCE_ERROR_CODE 	= 20;
	const int APP_VERSION_ERROR 		= 31;
	const int UNKNOWN_ERROR_CODE = 255;
	const string JSON_ERROR_CODE = "error_code";

	private int m_intTimeoutTime;
	private bool IS_STAND_ALONE = false;
	public bool isStandAlone(){
		return IS_STAND_ALONE;
	}

	public bool IsMaintenance;
	public bool IsSessionError;

	public enum EConnect{
		DISCONNECT			= 1,
		CONNECTING			= 2,
		CONNECTED			= 3,

		ERROR_REQUEST		= 11,
		ERROR_TIMEOUT		,
		ERROR_DATA_EMPTY	,
		ERROR_RECIEVED 		,
		ERROR_APP_VERSION	,		// アプリのバージョンが違う
		ERROR_UNKNOWN		,

		MAX					= 99,
	};

	public enum EStatus {
		SLEEP		= 0,
		INITIALIZE	,
		OK			,
		MAX			,
	};

	public EConnect	m_eConnect;
	public EStatus	m_eStatus;					// this class controll status
	public int m_intErrorCode;
	public int ErrorCode {
		get{ return m_intErrorCode;}
	}

	public IDictionary m_dictRecievedData;
	public bool m_bErrorLog;

	public bool SESSION_ERROR;

	// 接続できたかどうかの確認のみ（エラー状態は特に検出せず）
	public bool isConnected(){
		if( m_eConnect == EConnect.CONNECTED ){
			if( m_bErrorLog ){
				Debug.Log("success connected!" );
			}
			return true;
		}
		else {
			if( m_bErrorLog ){
				Debug.Log("false" );
			}
			return false;
		}
	}

	// 現在の通信接続状態を取得
	public EConnect getConnectStatus(){
		return m_eConnect;
	}

	// エラー状態の真偽を返す
	public bool isError(){
		bool bRet = false;
		switch(m_eConnect)
		{
			case EConnect.ERROR_REQUEST:
			case EConnect.ERROR_TIMEOUT:
			case EConnect.ERROR_DATA_EMPTY:
			case EConnect.ERROR_RECIEVED:
			case EConnect.ERROR_UNKNOWN:
				bRet = true;
				Debug.Log("Network Error!");
				break;
			default:
				break;
		}
		return bRet;
	}

	// 受信データ
	public IDictionary getData(){
		// 空の辞書データを用意したい
		return m_dictRecievedData;
	}

	public void setErrorLog( bool _bSet ){
		m_bErrorLog = _bSet;
		return;
	}

	public void SetTimeoutTime( int _intTime ){
		m_intTimeoutTime = _intTime;
		return;
	}

	public void request( E_NETWORK _eNetwork , string _strJson , int _intSerial = 0){
		if( m_bErrorLog ){
			Debug.Log("start recieve");
		}
		m_eConnect = EConnect.CONNECTING;
		StartCoroutine (send_module (_eNetwork , _strJson , _intSerial ));
		return;
	}

	public void request( E_NETWORK _eNetwork ){
		request( _eNetwork , "{\"dummy\":0}" , 0 );
		return;
	}

	IEnumerator send_module (E_NETWORK _eNetwork, string _strJson, int _intSerial)
	{
		// jsonのデータを生成
		string json_send = "dummy";

		string strHead = "";

		m_eStep = STEP.DELAY;

		switch (_eNetwork) {
		default:
			_strJson = _strJson.Remove (0, 1);
			_strJson = strHead + _strJson;
			break;
		}

		// 通信用に収納
		WWWForm form = new WWWForm ();
		form.AddField ("post_field", _strJson);

		string address = "";
		string host = "";
		host = "http://192.168.33.10/";
		setConnectingDialogTime ();

		switch( _eNetwork )
		{
			case E_NETWORK.TEST:
				address="mypage/test";
				_strJson = "";
				break;
			default:
				address="error";
				break;
		}

		string url = host + address;

		url = host;
		Debug.Log(url + _strJson);

		string strOutput = "";

		WWW www = new WWW( url ,form); //何も返ってこない場合

		// リクエストを受け取る処理(別メソッドでタイムアウト判定) 
		yield return StartCoroutine(ResponseConnectJson(www, (float)m_intTimeoutTime));

		//  リクエストエラーの場合
		if(!string.IsNullOrEmpty(www.error)){

			m_eStep = STEP.IDLE;
			m_intErrorCode = UNKNOWN_ERROR_CODE;
			m_eConnect = EConnect.ERROR_UNKNOWN;

			Debug.Log(string.Format("Fail Whale!\n{0}", www.error));
			yield break; // コルーチンを終了
		
			//タイムアウトエラーだった場合
		} else if (m_eConnect == EConnect.ERROR_TIMEOUT){
			if( m_bErrorLog ){
				Debug.Log("timeout_error");
			}
			m_eStep = STEP.IDLE;
			yield break; // コルーチンを終了

			//リクエストしたのに空で返ってきた場合
		} else if(string.IsNullOrEmpty(www.text)){
			if( m_bErrorLog ){
				Debug.Log("no items");
			}
			m_eStep = STEP.IDLE;
			yield break; // コルーチンを終了
		}
		else {
			int limit_string = 5000;
			string debug_json = www.text;
			bool bOver1000 = false;
			if( limit_string < debug_json.Length ){
				debug_json = "[over" + limit_string +"]" + debug_json.Substring(0 , limit_string);
				bOver1000 = true;
			}
			string decodedText = "";
			strOutput = www.text;
			Debug.Log("success:" + debug_json);
		}

		if( m_bErrorLog ){
			Debug.Log("set connected");
		}

		m_dictRecievedData = (IDictionary)Json.Deserialize(strOutput);
		bool bIsError = false;
		bool bErrorCode = false;
		m_intErrorCode = 0;
		if( bErrorCode ){
			if( 0 < m_intErrorCode ){
				bIsError = true;
			}
		}

		if( bIsError ){
			if( m_intErrorCode == MAINTENANCE_ERROR_CODE ){
				// 状態は変更しません
				IsMaintenance = true;
			}
			else if( m_intErrorCode == SESSION_ERROR_CODE){
				IsSessionError = true;
			}
			else {
				m_eConnect = EConnect.ERROR_RECIEVED;
			}
		}
		else {
			m_eConnect = EConnect.CONNECTED;
		}
		m_eStep = STEP.IDLE;

		yield break;		// コルーチン終了
	}

	private IEnumerator ResponseConnectJson(WWW www, float timeout){

		float requestTime = Time.time;
		while(!www.isDone)
		{
			if(Time.time - requestTime < timeout){
				yield return null;
			}
			else {
				Debug.LogWarning("TimeOut"); //タイムアウト
				m_eConnect = EConnect.ERROR_TIMEOUT;		// エラー状態にする
				break;
			}
		}
		yield return www;
	}

	public enum STEP{

		NONE 		= 0,
		IDLE 		,

		DELAY 		,
		BUSY 		,
		END 		,

		MAX 		,
	}
	public STEP m_eStep;
	public STEP m_eStepPre;
	public float m_fTimer;
	public float m_fConnectingDialogTime;
	public void setConnectingDialogTime (float _connectingDialogTime = 3.0f) {
		m_fConnectingDialogTime = _connectingDialogTime;
	}

	public GameObject m_goPosConnecting;

	public GameObject m_goConnecting;

	void Update (){

		bool bInit = false;
		if( m_eStepPre != m_eStep ){
			m_eStepPre  = m_eStep;
			bInit = true;
		}

		switch( m_eStep )
		{
		case STEP.NONE: break;

		case STEP.IDLE:
			if( bInit ){
			}
			break;

		case STEP.DELAY:
			if( bInit ){
				m_fTimer = 0.0f;
			}
			if( m_fConnectingDialogTime <= m_fTimer ){
				m_eStep = STEP.BUSY;
			}
			else {
				m_fTimer += Time.deltaTime;
			}
			break;

		case STEP.BUSY:
			if( bInit ){
			}
			break;
		case STEP.END:
		default:
			break;

		}
		return;
	}


	// ====================================================================================
	// 以下シングルトン宣言
	private static CommonNetwork instance = null;
	public static CommonNetwork Instance {
		get{
			if( instance == null ){
				instance = (CommonNetwork) FindObjectOfType(typeof(CommonNetwork));
				if (instance == null)
				{
					GameObject obj = new GameObject();
					obj.name = "CommonNetwork";
					obj.AddComponent<CommonNetwork>();

					instance = obj.GetComponent<CommonNetwork>();
					instance.Init();
					Debug.Log( "Create CommonNetwork");
					//Debug.LogError("CommonNetwork Instance Error");
				}
			}
			return instance;
		 }
	}
	protected void Init(){

		CommonNetwork[] obj = FindObjectsOfType(typeof(CommonNetwork)) as CommonNetwork[];
		if( obj.Length > 1 ){
			// 既に存在しているなら削除
			Destroy(gameObject);
		}else{
			// 音管理はシーン遷移では破棄させない
			DontDestroyOnLoad(gameObject);
		}
		m_eConnect= EConnect.DISCONNECT;
		m_eStatus = EStatus.SLEEP;
		m_bErrorLog = false;
		m_fTimer = 0.0f;

		SESSION_ERROR = false;
		m_intTimeoutTime = TIMEOUT_TIME;
		return;
	}
	void Awake(){
		return;
	}
	
}



