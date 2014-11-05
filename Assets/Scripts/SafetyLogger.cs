using UnityEngine;
using System.Collections;

/**
	使い方
	１．このソースをユニティのプロジェクトに放り込む
	２．SafetyLogger.Instance.Write(ここにログの文字列を追加);
	３．コンソール画面を見る！おしまい！

	ユーザーカスタムを行い場所
	①　継承するクラス名を変更
	　ログ出力を必要とする場合
	　　ーSafetyLoggerWriteを継承する
	　リリース時にログを出さないようにしたい場合
	　　ーSafetyLoggerNoneを継承する

	②　分割する文字数を変更したい場合
	　実行前に変更したい場合
	　　SafetyLoggerCoreクラス内のm_iLimitStringCountを変更する

	☓こちらはシングルトンにしたりなんだりしないと使えませんので、現在のバージョンでは利用しないでください
	　動的に変更したい場合
	　　SetLimitCountを呼び出して文字数を変更する。
	　　◯特に制限を設けていないのですが、大きすぎるとUnityが止まってしまうかもしれませんので
	　　 変更後はご注意ください

	関数名の規則
	直接呼び出すことがないもの
	　アンダーバー + 小文字始まり

	利用者が呼び出すもの
	　大文字始まり

	同様のクラス名があってうまくゆかない場合は
	ネームスペースなどを利用してください
*/
public abstract class SafetyLoggerCore : MonoBehaviour {
	public int m_iLimitStringCount = 5000;
	protected abstract int _write( string _strLog );

	/**
		1ログ分の文字数を変更できます。
		負の値を設定することはできません
	*/
	public void SetLimitCount( int _iLimit ){
		if( 0 < _iLimit ){
			m_iLimitStringCount = _iLimit;
		}
		return;
	}
}

public class SafetyLoggerWrite : SafetyLoggerCore {
	protected override int _write( string _strLog ){

		int iDivCount = (_strLog.Length / m_iLimitStringCount) + 1;	// ちょっと不自然ですが、割り算の切り上げというしょりでも良いです
		for( int i = 0 ; i < iDivCount ; i++ ){
			int iBuf = System.Math.Min( _strLog.Length - (i*m_iLimitStringCount), m_iLimitStringCount);
			Debug.Log( _strLog.Substring( i*m_iLimitStringCount , iBuf ));
		}
		return iDivCount;
	}
}

public class SafetyLoggerNone : SafetyLoggerCore {
	protected override int _write( string _strLog ){
		return 0;
	}
}

// このクラスがWrite or None のいずれかを継承して切り替える
public class SafetyLogger : SafetyLoggerWrite {
	private static SafetyLogger instance = null;
	public static SafetyLogger Instance {
		get{
			if( instance == null ){
				instance = (SafetyLogger) FindObjectOfType(typeof(SafetyLogger));
				if (instance == null)
				{
					GameObject obj = new GameObject();
					obj.name = "SafetyLogger";
					obj.AddComponent<SafetyLogger>();
					instance = obj.GetComponent<SafetyLogger>();
					Debug.Log( "Create SafetyLogger");
				}
			}
			return instance;
		 }
	}

	public int Write( string _strLog ){
		return _write(_strLog);
	}
}






