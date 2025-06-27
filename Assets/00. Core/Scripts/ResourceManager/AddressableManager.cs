using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class AddressableManager : MonoBehaviour
{
    public IEnumerator Start()
    {
        yield return InitAddressable();
        yield return TotalDownload();
    }
    // 어드레서블 초기화 루틴
    // Init -> CheckCatalog -> UpdateCatalog
    private IEnumerator InitAddressable()
    {
        // Init
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        Debug.Log("InitHandle 완");

        // CheckCatalog
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (checkHandle.Result.Count > 0)
            {
                Debug.Log("카탈로그 업데이트");
                foreach (var s in checkHandle.Result)
                {
                    Debug.Log(s);
                }

                // UpdateCatalog
                var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
                yield return updateHandle;
            }
            else
            {
                Debug.Log("카탈로그 업데이트 갯수 0");
            }
        }
        else
        {
            Debug.LogError("카탈로그 업데이트 실패");
        }

        Addressables.Release(checkHandle);
    }

    // 에셋 일괄 다운로드
    private IEnumerator TotalDownload()
    {
        List<object> allKeys = new List<object>();
        // foreach (var locator in Addressables.ResourceLocators)
        // {
        //     allKeys.AddRange(locator.Keys);
        // }
        allKeys.Add("Preload");
        // 4. 총 다운로드 용량 확인
        var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
        yield return sizeHandle;

        if (sizeHandle.Result > 0)
        {
            Debug.Log($"총 다운로드 용량: {sizeHandle.Result / (1024f * 1024f):0.00} MB");

            // 5. 전체 다운로드 수행
            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, false);
            int progress = 0;
            float time = 0;
            while (!downloadHandle.IsDone)
            {
                time += Time.deltaTime;
                var s = downloadHandle.GetDownloadStatus();
                var p = (int)(s.Percent * 100);
                if (p > progress || time > 1)
                {
                    Debug.Log($"다운로드 진행률: {p:0.0}%");
                    progress = p;
                    time = 0;
                }

                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("에셋 다운로드 완료!");
            }
            else
            {
                Debug.LogError("에셋 다운로드 실패");
            }

            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log("다운로드할 에셋이 없습니다");
        }

        yield return null;
        Addressables.Release(sizeHandle);
    }

    // 에셋 분할 다운로드
    // 이렇게 하는이유? 진행상황 표시하기 편해서
    private IEnumerator SplitDownload()
    {
        List<object> allKeys = new List<object>();
        allKeys.Add("Preload");
        var totalSize = 0l;
        var totalCount = 0;
        var splitKeys = new Dictionary<object, float>();
        foreach (var key in allKeys)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(key);
            yield return sizeHandle;
            if (sizeHandle.Result <= 0)
            {
                continue;
            }

            splitKeys.Add(key, sizeHandle.Result);
            totalSize += sizeHandle.Result;
            totalCount++;
            Addressables.Release(sizeHandle);
        }

        if (totalCount > 0)
        {
            Debug.Log($"총 다운로드 용량: {totalSize / (1024f * 1024f):0.00} MB");

            int count = 0;
            long downloadSize = 0;
            foreach (var key in splitKeys.Keys)
            {
                count++;
                var downloadHandle = Addressables.DownloadDependenciesAsync(key, false);
                int progress = 0;
                float time = 0;
                while (downloadHandle.IsDone == false)
                {
                    time += Time.deltaTime;
                    var s = downloadHandle.GetDownloadStatus();
                    var p = (int)(s.Percent * 100);
                    if (p > progress || time > 1)
                    {
                        Debug.Log($"다운로드 진행률: {p:0.0}% {count}/{totalCount}");
                        Debug.Log($"전체 진행률: {(downloadSize + s.DownloadedBytes) / (1024f * 1024f):0.00} / {totalSize / (1024f * 1024f):0.00} MB");
                        progress = p;
                        time = 0;
                    }

                    yield return null;
                }
                
                if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("에셋 다운로드 완료!");
                }
                else
                {
                    Debug.LogError("에셋 다운로드 실패");
                }

                downloadSize += downloadHandle.GetDownloadStatus().TotalBytes;

                Addressables.Release(downloadHandle);
            }
        }
        else
        {
            Debug.Log("다운로드할 에셋이 없습니다");
        }
    }
    
    private Dictionary<string, AsyncOperationHandle> _handleDict = new Dictionary<string, AsyncOperationHandle>();
    public IEnumerator LoadAsset<T>(string key, Action<T> onFinish) where T : Object
    {
        Debug.Log($"LoadAsync {key}");
        
        AsyncOperationHandle handle;
        if (_handleDict.ContainsKey(key) == false)
        {
            handle = Addressables.LoadAssetAsync<T>(key);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"LoadAsync {key} Succeeded");
                _handleDict[key] = handle;    
            }
            else
            {
                Debug.LogError($"LoadAsync {key} Failed");
                yield break;
            }
        }
        else
        {
            Debug.Log($"Load cached asset {key}");
            handle = _handleDict[key];
        }
        
        var result = handle.Result as T;
        if (result != null)
        {
            Debug.Log($"Invoke OnFinish {key}");
            onFinish?.Invoke(result);
        }
    }
}