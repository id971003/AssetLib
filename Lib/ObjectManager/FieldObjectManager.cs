using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic.Systems
{
    public class FieldObjectManager
    {
        public ResourceSystem m_ResSys;
        public ObjectPoolManager FieldObjectPoolManager => m_FieldObjectPoolManager;
        private ObjectPoolManager m_FieldObjectPoolManager;

        private int m_FieldObjectAutoIncrement;

        public Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> AllFieldObjectDict => m_allFieldObjectDict;
        private Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> m_allFieldObjectDict;

        public Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> TargetFieldObjectDict => m_targetFieldObjectDict;
        private Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> m_targetFieldObjectDict;

        public FieldObjectManager()
        {
            m_ResSys = ResourceSystem.Instance;
            m_FieldObjectPoolManager = new ObjectPoolManager();
            m_allFieldObjectDict = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();
            m_targetFieldObjectDict = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();
            m_FieldObjectAutoIncrement = 0;
            InitCommonPrefab();
        }

        public void ClearAllFieldObject()
        {
            // 모든 FieldObject들을 풀에 반납
            if (m_allFieldObjectDict != null)
            {
                foreach (var teamDict in m_allFieldObjectDict.Values)
                {
                    foreach (var fieldObj in teamDict.Values)
                    {
                        m_FieldObjectPoolManager.Release(fieldObj);
                    }
                }
            }

            // 딕셔너리 초기화
            m_allFieldObjectDict = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();
            m_targetFieldObjectDict = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();
            m_FieldObjectAutoIncrement = 0;


            BattleManager.Instance.CharacterMgr.ClearManagerEachFieldObjectList();
            BattleManager.Instance.MonsterMgr.ClearManagerEachFieldObjectList();

            
        }

        public bool ClearFieldObject(FieldObject _target)
        {
            bool Result = false;
            if (m_allFieldObjectDict.TryGetValue(_target.NewTemType, out var teamDict))
            {
                teamDict.Remove(_target.ObjectIndex);
                Result = true;
            }
            //혹시 있으면
            if (m_targetFieldObjectDict.TryGetValue(_target.NewTemType, out var targetTeamTict))
            {
                targetTeamTict.Remove(_target.ObjectIndex);
                Result = true;
            }
            // 풀에 반납
            m_FieldObjectPoolManager.Release(_target);
            return Result;
        }

        public void InitCommonPrefab()
        {
            FieldObject characterBase = m_ResSys.LoadResource<GameObject>(ResourcePathType.Addressable, "FieldObject/Prefab/Base/Field_CharacterBase.prefab").GetComponent<FieldObject>();
            
            m_FieldObjectPoolManager.RegisterPrefabFromPath<FieldObject>("CharacterBase", characterBase, 5);

            //몬스터
            FieldObject commonMonster = m_ResSys.LoadResource<GameObject>(ResourcePathType.Addressable, "FieldObject/Prefab/Base/Field_MonsterBase.prefab").GetComponent<FieldObject>();
            m_FieldObjectPoolManager.RegisterPrefabFromPath("MonsterBase", commonMonster, 30);

          
        }

        public FieldObject CreateFieldObjct(ObjectTeamType type, string Key, bool _isTargetable, ref int FiedObjectIndex)
        {
            var target = m_FieldObjectPoolManager.Create<FieldObject>(Key);

            if (target == null)
            {
                Debug.Log("Key 이상함" + Key);
                return null;
            }
            target.NewTemType = type;
            target.ObjectIndex = m_FieldObjectAutoIncrement;
            m_allFieldObjectDict.TryGetValue(type, out var fieldObjectDict);

            if (fieldObjectDict == null)
            {
                fieldObjectDict = new Dictionary<int, FieldObject>();
                m_allFieldObjectDict.Add(type, fieldObjectDict);
            }
            fieldObjectDict.Add(m_FieldObjectAutoIncrement, target);

            // 타겟 가능한 오브젝트면 타겟 딕셔너리에도 추가
            if (_isTargetable)
            {
                m_targetFieldObjectDict.TryGetValue(type, out var targetFieldObjectDict);
                if (targetFieldObjectDict == null)
                {
                    targetFieldObjectDict = new Dictionary<int, FieldObject>();
                    m_targetFieldObjectDict.Add(type, targetFieldObjectDict);
                }
                targetFieldObjectDict.Add(m_FieldObjectAutoIncrement, target);
            }
            FiedObjectIndex = m_FieldObjectAutoIncrement;
            m_FieldObjectAutoIncrement++;
            return target;
        }

        #region 찾기

        #region 유틸
        public double GetTargetDistance(FieldObject _base, FieldObject _taget)
        {
            Vector2 gap_pos = _taget.GetFootCenterPoint() - _base.GetFootCenterPoint();
            gap_pos.y *= 3f;

            float distance = Math.Abs(Vector2.Distance(Vector2.zero, gap_pos));
            return (double)distance;
        }
        public double GetTargetDistance(FieldObject _base, Vector2 _target_pos)
        {
            Vector2 gap_pos = _target_pos - _base.GetFootCenterPoint();
            gap_pos.y *= 1.66f;

            double distance = Math.Abs(Vector2.Distance(Vector2.zero, gap_pos));
            return distance;
        }
        #endregion

        #region 내타입말고 다른 타입 찾기
        public Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> GetOtherFindFieldObjectType(FieldObject _target)
        {
            if (_target == null)
                return null;
            ObjectTeamType type = _target.NewTemType;

            var result = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();

            foreach (ObjectTeamType teamType in System.Enum.GetValues(typeof(ObjectTeamType)))
            {
                if (teamType == type) continue;

                if (m_targetFieldObjectDict.TryGetValue(teamType, out var dict))
                {
                    result.Add(teamType, dict);
                }
            }

            return result;
        }
        public Dictionary<ObjectTeamType, Dictionary<int, FieldObject>> GetOtherFindFieldObjectType(ObjectTeamType _type)
        {
            var result = new Dictionary<ObjectTeamType, Dictionary<int, FieldObject>>();

            foreach (ObjectTeamType teamType in System.Enum.GetValues(typeof(ObjectTeamType)))
            {
                if (teamType == _type) continue;

                if (m_targetFieldObjectDict.TryGetValue(teamType, out var dict))
                {
                    result.Add(teamType, dict);
                }
            }

            return result;
        }
        #endregion

        #region 나랑 적인애들 찾기
        public bool GetAllActiveFieldObject(FieldObject _target, ref List<FieldObject> _result)
        {
            if (_result == null || _target == null)
                return false;

            _result.Clear();
            var enemyTeamDict = GetOtherFindFieldObjectType(_target);

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    _result.Add(pair.Value);
                }
            }

            return true;
        }

        public bool GetAllActiveAliveFieldObject(FieldObject _target, ref List<FieldObject> _result)
        {
            if (_result == null || _target == null)
                return false;

            _result.Clear();
            var enemyTeamDict = GetOtherFindFieldObjectType(_target);

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    if (pair.Value.IsDeath() == false && !pair.Value.IsWillDeath)
                        _result.Add(pair.Value);
                }
            }

            return true;
        }

        public FieldObject[] FindFieldObjectsWithInRange(FieldObject _target, Vector2 _standard_pos, double _distance)
        {
            if (_target == null)
                return new FieldObject[0];

            var enemyTeamDict = GetOtherFindFieldObjectType(_target);
            var result = new List<FieldObject>();

            foreach (var teamDict in enemyTeamDict.Values)
            {
                var inRangeObjects = teamDict.Values.Where(fieldObj =>
                    GetTargetDistance(fieldObj, _standard_pos) <= _distance && fieldObj.IsDeath() == false);
                result.AddRange(inRangeObjects);
            }

            return result.ToArray();
        }

        public FieldObject GetRandomFieldObject(FieldObject _target)
        {
            if (_target == null)
                return null;

            var enemyTeamDict = GetOtherFindFieldObjectType(_target);
            var allAliveEnemies = new List<FieldObject>();

            foreach (var teamDict in enemyTeamDict.Values)
            {
                var aliveEnemies = teamDict.Values.Where(fieldObj => fieldObj.IsDeath() == false);
                allAliveEnemies.AddRange(aliveEnemies);
            }

            if (allAliveEnemies.Count == 0)
                return null;

            int randIdx = UnityEngine.Random.Range(0, allAliveEnemies.Count);
            return allAliveEnemies[randIdx];
        }

        public FieldObject FindClosestFieldObject(FieldObject _target, Vector2 _standard_pos)
        {
            if (_target == null)
                return null;

            var enemyTeamDict = GetOtherFindFieldObjectType(_target);
            FieldObject closestFieldObject = null;
            double minDistance = double.MaxValue;

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    var fieldObj = pair.Value;
                    if (fieldObj.IsDeath() == false)
                    {
                        double distance = GetTargetDistance(fieldObj, _standard_pos);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestFieldObject = fieldObj;
                        }
                    }
                }
            }

            return closestFieldObject;
        }

        public FieldObject FindClosestFieldObject(FieldObject _standard, out double _distance)
        {
            _distance = 0;
            if (_standard == null)
                return null;

            var enemyTeamDict = GetOtherFindFieldObjectType(_standard);
            FieldObject closestFieldObj = null;
            double min_distance = -1;

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    var fieldObj = pair.Value;
                    if (fieldObj.IsDeath())
                        continue;

                    double distance = GetTargetDistance(_standard, fieldObj);

                    if (min_distance == -1 || distance < min_distance)
                    {
                        closestFieldObj = fieldObj;
                        min_distance = distance;
                    }
                }
            }

            _distance = min_distance;
            return closestFieldObj;
        }
        public FieldObject FindClosestFieldObject(FieldObject _standard, out double _distance, Vector2 _addPosition)
        {
            _distance = 0;
            if (_standard == null)
                return null;

            var enemyTeamDict = GetOtherFindFieldObjectType(_standard);
            FieldObject closestFieldObj = null;
            double min_distance = -1;

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    var fieldObj = pair.Value;
                    if (fieldObj.IsDeath())
                        continue;

                    double distance = GetTargetDistance(_standard, fieldObj);

                    if (min_distance == -1 || distance < min_distance)
                    {
                        closestFieldObj = fieldObj;
                        min_distance = distance;
                    }
                }
            }

            _distance = min_distance;
            return closestFieldObj;
        }

        public FieldObject FindClosestFieldObjectByXDistance(FieldObject _target, double _maxDistance)
        {
            if (_target == null)
                return null;

            var enemyTeamDict = GetOtherFindFieldObjectType(_target);
            FieldObject closestFieldObject = null;
            double minXDistance = double.MaxValue;

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    var fieldObj = pair.Value;
                    if (fieldObj.IsDeath())
                        continue;

                    // x축 거리만 계산
                    double xDistance = Math.Abs(_target.transform.position.x - fieldObj.transform.position.x);

                    // maxDistance 내에 있고 현재 최소거리보다 가까우면 업데이트
                    if (xDistance <= _maxDistance && xDistance < minXDistance)
                    {
                        minXDistance = xDistance;
                        closestFieldObject = fieldObj;
                    }
                }
            }

            return closestFieldObject;
        }
        #endregion

        #region 강제로 타입 찾기
        public bool GetAllActiveFieldObject(ObjectTeamType _findType, ref List<FieldObject> _result)
        {
            if (_result == null)
                return false;

            _result.Clear();

            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList != null)
            {
                foreach (var pair in fieldObjectList)
                {
                    _result.Add(pair.Value);
                }
            }

            return true;
        }

        public bool GetAllActiveAliveFieldObject(ObjectTeamType _findType, ref List<FieldObject> _result)
        {
            if (_result == null)
                return false;

            _result.Clear();

            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList != null)
            {
                foreach (var pair in fieldObjectList)
                {
                    if (pair.Value.IsDeath() == false && !pair.Value.IsWillDeath)
                        _result.Add(pair.Value);
                }
            }

            return true;
        }

        public FieldObject[] FindFieldObjectsWithInRange(ObjectTeamType _findType, Vector2 _standard_pos, double _distance)
        {
            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList == null)
                return new FieldObject[0];

            return fieldObjectList.Values.Where(fieldObj =>
               GetTargetDistance(fieldObj, _standard_pos) <= _distance && fieldObj.IsDeath() == false
            ).ToArray();
        }

        public FieldObject GetRandomFieldObject(ObjectTeamType _findType)
        {
            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList == null)
                return null;

            var aliveFieldObjects = fieldObjectList.Values
                .Where(fieldObj => fieldObj.IsDeath() == false)
                .ToList();

            if (aliveFieldObjects.Count == 0)
                return null;

            int randIdx = UnityEngine.Random.Range(0, aliveFieldObjects.Count);
            return aliveFieldObjects[randIdx];
        }

        public FieldObject FindClosestFieldObject(ObjectTeamType _findType, Vector2 _standard_pos)
        {
            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList == null)
                return null;

            var aliveFieldObjects = fieldObjectList.Values
                .Where(fieldObj => fieldObj.IsDeath() == false)
                .ToList();

            if (aliveFieldObjects.Count == 0)
                return null;

            FieldObject closestFieldObject = null;
            double minDistance = double.MaxValue;

            foreach (var fieldObj in aliveFieldObjects)
            {
                double distance = GetTargetDistance(fieldObj, _standard_pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestFieldObject = fieldObj;
                }
            }

            return closestFieldObject;
        }

        public FieldObject FindClosestFieldObject(ObjectTeamType _findType, FieldObject _standard, out double _distance)
        {
            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList == null || fieldObjectList.Count == 0)
            {
                _distance = 0;
                return null;
            }

            FieldObject closestFieldObj = null;
            double min_distance = -1;

            foreach (var pair in fieldObjectList)
            {
                var fieldObj = pair.Value;
                if (fieldObj.IsDeath())
                    continue;

                if (min_distance == -1)
                {
                    closestFieldObj = pair.Value;
                    min_distance = GetTargetDistance(_standard, closestFieldObj);
                }

                double distance = GetTargetDistance(_standard, pair.Value);
                if (distance < min_distance)
                {
                    closestFieldObj = pair.Value;
                    min_distance = distance;
                }
            }

            _distance = min_distance;
            return closestFieldObj;
        }
        #endregion
        #endregion

        #region 옮기기
        public void MoveTargetFieldObject(FieldObject _fieldObject, ObjectTeamType _newTeamType)
        {
            if (_fieldObject == null)
                return;

            ObjectTeamType currentTeamType = _fieldObject.NewTemType;

            if (m_allFieldObjectDict.TryGetValue(currentTeamType, out var currentAllDict))
            {
                currentAllDict.Remove(_fieldObject.ObjectIndex);
            }

            if (!m_allFieldObjectDict.TryGetValue(_newTeamType, out var newAllDict))
            {
                newAllDict = new Dictionary<int, FieldObject>();
                m_allFieldObjectDict.Add(_newTeamType, newAllDict);
            }
            newAllDict.Add(_fieldObject.ObjectIndex, _fieldObject);

            if (m_targetFieldObjectDict.TryGetValue(currentTeamType, out var currentTeamDict))
            {
                currentTeamDict.Remove(_fieldObject.ObjectIndex);
            }

            if (!m_targetFieldObjectDict.TryGetValue(_newTeamType, out var newTeamDict))
            {
                newTeamDict = new Dictionary<int, FieldObject>();
                m_targetFieldObjectDict.Add(_newTeamType, newTeamDict);
            }
            newTeamDict.Add(_fieldObject.ObjectIndex, _fieldObject);

            _fieldObject.NewTemType = _newTeamType;
        }
        #endregion

        #region 유틸
        public int GetAliveActiveFieldObjectCount(ObjectTeamType _findType, bool _exptSpecMonster = false)
        {
            int count = 0;

            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList != null)
            {
                foreach (var pair in fieldObjectList)
                {
                    if (pair.Value.IsDeath() == false)
                        count++;
                }
            }

            return count;
        }

        public bool IsAliveFieldObject(ObjectTeamType _findType)
        {
            m_targetFieldObjectDict.TryGetValue(_findType, out var fieldObjectList);
            if (fieldObjectList == null || fieldObjectList.Count == 0)
                return false;

            foreach (var pair in fieldObjectList)
            {
                if (pair.Value.IsWillDeath == false)
                    return true;
            }

            return false;
        }

        public bool IsAliveEnemy(ObjectTeamType _findType)
        {
            var enemyTeamDict = GetOtherFindFieldObjectType(_findType);
            if (enemyTeamDict == null || enemyTeamDict.Count == 0)
                return false;

            foreach (var teamDict in enemyTeamDict.Values)
            {
                foreach (var pair in teamDict)
                {
                    if (pair.Value.IsWillDeath == false)
                        return true;
                }
            }

            return false;
        }
        #endregion

        public void RunUpdate(float _deltaTime)
        {
        }
    }
}