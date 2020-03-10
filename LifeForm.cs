using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeForm : MonoBehaviour
{
    // Start is called before the first frame update
    [Range(0, 100)]
    public float reproductionRate; //Chance to reproduce at each step
    [Range(0, 100)]
    public float reproductionVariance;  //the potential change in reproductionRate

    [Range(0, 100)]
    public float deathRate;        //Chance to Die at each step
    [Range(0, 100)]
    public float deathVariance;   
    
    [Range(0, 100)]
    public float mutationChance;   //Chance there will be a mutation after reproduction
    [Range(0, 100)]
    public float mutationChanceVariance;

    [Range(0, 100)]
    public float moveSpeed;
    [Range(0, 100)]
    public float moveSpeedVariance;

    [Range(0, 100)]
    public float vision;
    [Range(0, 100)]
    public float visionVariance;

    [Range(0, 100)]
    public float size;
    [Range(0, 100)]
    public float sizeVariance;

    MeshRenderer meshRenderer;
    public Material mat;
    public Color matColor { get
                            {
                                if (!mat)
                                {
                                    mat = meshRenderer.material;
                                }
                                return mat.color;
                            }
                            set
                            {
                                mat = new Material(mat);
                                mat.color = value;
                            }
    }
    [Range(0, 255)]
    public int colorVariance;

    [Range(0, 100)]
    public float hunger;        //How much the life form can eat
    [Range(0, 100)]
    public float hungerVariance;

    public float eatRange;  //how close something has to be to get eaten
    public float eatRangeVariance;

    float nutrition;        //how much this lifeform has eaten. Being at 0 for 2 steps is instant death

    void Start()
    {
        nutrition = hunger;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Step()
    {
        if(Random.Range(0.0f, 100f) <= (WorldProperties.world.overPopulated ? deathRate  * 2: deathRate)) { Invoke("Die", 0.016f); }
        else if(Random.Range(0.0f, 100f) <= reproductionRate) { Reproduce(); }
    }

    void Reproduce()
    {
        LifeForm offspring = (LifeForm)Instantiate(this, GetOffspringPosition(), Quaternion.identity);//Todo Make an entity
        if(Random.Range(0f, 100f) < mutationChance)
        {
            //Spawn another LifeForm with a varied stat
            int rand = Random.Range(1, 7);

            switch (rand)
            {
                case 1:
                    offspring.MutateReproductionRate();
                    break;
                case 2:
                    offspring.MutateDeathRate();
                    break;
                case 3:
                    offspring.MutateMutationChance();
                    break;
                case 4:
                    offspring.MutateMoveSpeed();
                    break;
                case 5:
                    offspring.MutateVision();
                    break;
                case 6:
                    offspring.MutateSize();
                    break;
                case 7:
                    offspring.MutateEatRange();
                    break;
                default:
                    break;
            }

            offspring.MutateColor();
        }
        WorldProperties.world.newBornQueue.Add(offspring);
    }

    void MutateReproductionRate()
    {
        float rate = reproductionRate + Random.Range(-reproductionVariance, reproductionVariance);

        reproductionRate =  Mathf.Clamp(rate, 0, 100);
    }
    void MutateDeathRate()
    {
        float rate = deathRate + Random.Range(-deathVariance, deathVariance);

        deathRate = Mathf.Clamp(rate, 0, 100);
    }
    void MutateMutationChance()
    {
        float rate = mutationChance + Random.Range(-mutationChanceVariance, mutationChanceVariance);

        mutationChance = Mathf.Clamp(rate, 0, 100);
    }
    void MutateMoveSpeed()
    {
        float rate = moveSpeed + Random.Range(-moveSpeedVariance, moveSpeedVariance);

        size = Mathf.Clamp(size, 0, 100 - ((50 - rate) * (50 - rate)) * 0.04f);

        hunger = Mathf.Clamp(hunger, 0, Mathf.Clamp(hunger + 0.25f * rate, 0, 100));

        moveSpeed = Mathf.Clamp(rate, 0, 100);
    }
    void MutateVision()
    {
        float rate = vision + Random.Range(-visionVariance, visionVariance);

        vision = Mathf.Clamp(rate, 0, 100);
    }
    void MutateSize()
    {
        float rate = size + Random.Range(-sizeVariance, sizeVariance);

        moveSpeed = Mathf.Clamp(moveSpeed, 0, Mathf.Clamp(100 - ((50 - rate) * (50-rate)) * 0.04f, 0, 100));
        hunger = Mathf.Clamp(hunger, 0, Mathf.Log10(2 * rate) * 43.5f);
        nutrition = hunger;

        size = Mathf.Clamp(rate, 0, 100);

        transform.localScale = new Vector3(size * 0.1f, size * 0.1f, size * 0.1f);
    }
    void MutateColor()
    {
        Vector3Int colorChange = new Vector3Int(Random.Range(-colorVariance, colorVariance), 
                                                Random.Range(-colorVariance, colorVariance), 
                                                Random.Range(-colorVariance, colorVariance));

        matColor = new Color(Mathf.Clamp(matColor.r + colorChange.x, 0, 255),
                                Mathf.Clamp(matColor.g + colorChange.y, 0, 255),
                                Mathf.Clamp(matColor.b + colorChange.z, 0, 255));
    }
    //void MutateHunger()
    //{
    //    float rate = hunger + Random.Range(-hungerVariance, hungerVariance);

    //    hunger = Mathf.Clamp(rate, 0, 100);

    //    nutrition = hunger;
    //}
    void MutateEatRange()
    {
        float rate = eatRange + Random.Range(-eatRangeVariance, eatRangeVariance);

        eatRange = Mathf.Clamp(rate, 0, size * 1.5f);
    }


    Vector3 GetOffspringPosition()
    {
        Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        offset.Normalize();
        return transform.position + ((Vector3)offset * size * 0.1f);
    }

    void Die()
    {
        WorldProperties.world.life.Remove(this);
        Destroy(gameObject);
    }

    public float GetEaten()
    {
        float carbs = nutrition + size * 0.5f;
        Invoke("Die", 0.016f);
        return carbs;
    }
}
