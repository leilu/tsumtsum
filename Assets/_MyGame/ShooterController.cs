using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShooterController : MonoBehaviour
{
    public GameObject[] ballPrefabs;
    public float ballSize = 0.85f;
    public float dragBetweenDistanceConstant = 2.5f;

    // 最初にドラッグしたボール
    private GameObject firstBall;
    // 削除するボールのリスト
    private ArrayList removableBallList;
    // 直前にドラッグしたボール
    private GameObject lastBall;
    // 現在リストにあるボールの名前(色)
    private string currentName;

    //--UI用
    // プレイ中?
    private bool isPlaying = false;
    // タイマ
    public GameObject timerObj;
    private Text timerText;
    // 制限時間
    public float timeLimit = 60.0f;
    // カウントダウン
    public float countDownTime = 5.0f;

    public GameObject scoreObj;
    private Text scoreText;
    public int currentScore = 0;


    // Use this for initialization
    void Start()
    {
        // Timer
        timerText = timerObj.GetComponent<Text>();
        // Score
        scoreText = scoreObj.GetComponent<Text>();
        
        // カウント開始
        StartCoroutine(CountDown());
        // ボール生成
        StartCoroutine(Shootballs(60));
    }

    // Update is called once per frame
    void Update()
    {
        // isPlayng判定
        if (isPlaying)
        {
            // ボールのドラッグ開始
            if (Input.GetMouseButton(0) && firstBall == null)
            {
                OnDragStart();
            }
            // ボールのドラッグ終了
            else if (Input.GetMouseButtonUp(0))
            {
                OnDragEnd();
            }
            // ボールをドラッグしている状態
            else if (firstBall != null)
            {
                OnDragging();
            }
        }
        // score更新
        scoreText.text = "SCORE:" + currentScore;
    }

    IEnumerator CountDown()
    {
        float count = countDownTime;
        while (count > 0)
        {
            // タイマー更新
            timerText.text = count.ToString();
            // 1秒wait
            yield return new WaitForSeconds(1);
            // カウント減
            count--;
        }
        timerText.text = "Start!!";
        isPlaying = true;
        yield return new WaitForSeconds(1);
        // 制限時間のカウント開始
        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        float limitCount = timeLimit;
        while (limitCount > 0)
        {
            timerText.text = limitCount.ToString();
            yield return new WaitForSeconds(1);
            limitCount--;
        }
        timerText.text = "finish";
        // プレイ強制終了
        OnDragEnd();
        isPlaying = false;
    }

    void OnDragStart()
    {
        Collider col = GetCurrentHitCollider();
        if (col != null)
        {
            GameObject colObj = col.gameObject;
            // .IndexOf("Ball")：名前に"Ball"を含む（-1で含まない）
            if (colObj.name.IndexOf("Ball") != -1)
            {
                // 削除するボールのリストを初期化
                removableBallList = new ArrayList();
                // ドラッグしたボール ＝ 最初のボール
                firstBall = colObj;
                // ドラッグしたボールの色(名前)
                currentName = colObj.name;
                // 削除リストへ追加
                PushToList(colObj);
            }
        }

    }

    // rayが衝突したコライダーの情報を得る
    Collider GetCurrentHitCollider()
    {
        // mouseのRayを作成
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Rayが衝突したかどうか
        if (Physics.Raycast(mouseRay, out hit))
        {

            /*/ Examples
            // 衝突したオブジェクトの色を赤に変える
            hit.collider.GetComponent<MeshRenderer>().material.color = Color.red;
            // 衝突したオブジェクトを消す
            Destroy(hit.collider.gameObject);
            // Rayの衝突地点に、このスクリプトがアタッチされているオブジェクトを移動させる
            this.transform.position = hit.point;
            // Rayの原点から衝突地点までの距離を得る
            float dis = hit.distance;
            // 衝突したオブジェクトのコライダーを非アクティブにする
            hit.collider.enabled = false;
            */
            return hit.collider;
        }
        return null;
    }

    void PushToList(GameObject obj)
    {
        lastBall = obj;
        removableBallList.Add(obj);
        // 区別用
        obj.name = "_" + obj.name;

        // 色変え
        StartCoroutine(ChangeColor(obj, 0.2f));
    }
    void OnDragging()
    {
        Collider col = GetCurrentHitCollider();
        if (col != null)
        {
            GameObject colObj = col.gameObject;
            // 現在リストに追加されている色とcolが同じ
            if (colObj.name == currentName)
            {
                // 直前にリストへ追加したボールではないものをドラッグしているとき
                if (lastBall != colObj)
                {
                    // 距離を計算
                    float dist = Vector3.Distance(lastBall.transform.position,
                                                 colObj.transform.position);
                    if (dist <= (ballSize * dragBetweenDistanceConstant))
                    {
                        // 削除対象に追加
                        PushToList(colObj);
                    }

                }
            }
        }
    }
    void OnDragEnd()
    {
        // なぞっているボールが複数
        if (firstBall != null)
        {
            int length = removableBallList.Count;
            // 3個以上なら、削除
            if (length >= 3)
            {
                for (int i = 0; i < length; i++)
                {
                    // 削除
                    Destroy((GameObject)removableBallList[i]);
                }
                // Score算出
                // Ballの固有スコアは一律50点
                currentScore += (CalcBaseScore(length) + 50 * length);
                // 消した分だけ生成
                StartCoroutine(Shootballs(length));
            }
            // 3個以上ボールがない
            else
            {
                for (int i = 0; i < length; i++)
                {
                    GameObject listedBall = (GameObject)removableBallList[i];
                    // 名前を元に戻す
                    listedBall.name = listedBall.name.Substring(1, 5);
                    // 透過を元に戻す
                    StartCoroutine(ChangeColor(listedBall, 1.0f));
                }
            }
            // 変数を初期化
            firstBall = null;
        }
    }

    IEnumerator Shootballs(int dropCount)
    {
        for (int i = 0; i < dropCount; i++)
        {
            // Prefabs[]からランダム生成
            int ballPrefabNumber = Random.Range(0, ballPrefabs.Length);
            GameObject ball = Instantiate(ballPrefabs[ballPrefabNumber]);
            // 生成位置
            Vector3 tempPosition = ball.transform.position;
            tempPosition.x = Random.Range(tempPosition.x - 0.85f, tempPosition.x + 0.85f);
            tempPosition.y = tempPosition.y + 10.25f;//調整
            ball.transform.position = tempPosition;

            // 名前
            ball.name = "Ball" + ballPrefabNumber;

            // 次のボール生成まで待つ
            // 指定した秒数待つ関数：WaitForSeconds(float)
            yield return new WaitForSeconds(0.05f);
        }
    }

    // ボールの色とかを変えます
    // つかいかた：StartCoroutine(ChangeColor(obj, float));
    IEnumerator ChangeColor(GameObject obj, float transparency)
    {
        // オブジェクトのRenderer（画面の要素を管理している要素）を取得
        Renderer ballRenderer = obj.GetComponent<Renderer>();
        // アルファ値減算
        if (transparency != 1.0f)
        {
            for (float alpha = ballRenderer.material.color.a;
                        alpha >= transparency;
                        alpha -= 0.1f)
            {
                // ボールのColorをセット
                Color tempC = ballRenderer.material.color;
                tempC.a = alpha;
                ballRenderer.material.color = tempC;
                // 1Frame待機
                yield return null;
            }
        }
        else {
            // アルファ値加算
            for (float alpha = ballRenderer.material.color.a;
                        alpha <= transparency;
                        alpha += 0.1f)
            {
                // ボールのColorをセット
                Color tempC = ballRenderer.material.color;
                tempC.a = alpha;
                ballRenderer.material.color = tempC;
                // 1Frame待機
                yield return null;
            }
        }
    }
    // Score計算
    // ボールを消した時の得点は
    //      合計点＝基本スコア+(ボールの固有スコア)×消したボールの数
    // という式で表されます。 また基本スコアは消したボールの数をnとすると
    //      基本スコア=50×n(n+1)−300 となります。
    int CalcBaseScore(int dragBallNum)
    {
        int tempScore = 50 * dragBallNum * (dragBallNum + 1) - 300;
        return tempScore;
    }
}