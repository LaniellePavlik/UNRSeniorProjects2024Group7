using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class MathUtils
    {
        public static float Magnitude(float2 a, float2 b)
        {
            float2 c = b - a;
            return math.sqrt(c.x * c.x + c.y * c.y);
        }

        public static quaternion Abs(quaternion q)
        {
            return q.value.w < 0.0f ? new quaternion(-q.value.x, -q.value.y, -q.value.z, -q.value.w) : q;
        }

        public static float3 ToScaledAngleAxis(quaternion q1, float eps = 1e-8f)
        {
            return 2.0f * Log(q1, eps);
        }

        public static quaternion FromScaledAngleAxis(float3 angleAxis, float eps = 1e-8f)
        {
            return Exp(angleAxis * 0.5f, eps);
        }

        private static float3 Log(quaternion q, float epsilon)
        {
            float qLength = math.sqrt(q.value.x * q.value.x + q.value.y * q.value.y
                                                            + q.value.z * q.value.z);

            //Near zero length
            if (qLength < epsilon) return new float3(q.value.x, q.value.y, q.value.z);

            //Else scaled axis based on theta
            float theta = math.acos(math.clamp(q.value.w, -1f, 1f));
            return theta * (new float3(q.value.x, q.value.y, q.value.z) / qLength);
        }

        private static quaternion Exp(float3 angleAxis, float epsilon)
        {
            float theta = math.sqrt(angleAxis.x * angleAxis.x + angleAxis.y * angleAxis.y + angleAxis.z * angleAxis.z);

            //Zero rot quaternion is q = <1, 0, 0, 0>
            if (theta < epsilon) return new quaternion(angleAxis.x, angleAxis.y, angleAxis.z, 1.0f);

            //Else, create quat with theta
            float c = math.cos(theta);
            float s = math.sin(theta);
            float3 unitV = angleAxis / theta;

            //Q for theta = angle/2 is q = < cos(theta), sin(theta)*i, sin(theta)*j, sin(theta)*k)>
            return new quaternion(s * unitV.x, s * unitV.y, s * unitV.z, c);
        }

        private static quaternion QuaternionNormalized(quaternion q, float epsilon)
        {
            float divisor = (QuaternionLength(q) + epsilon);
            //Should be q / divisor...
            return new quaternion(q.value.x / divisor, q.value.y / divisor, q.value.z / divisor, q.value.w / divisor);
        }

        private static float QuaternionLength(quaternion q)
        {
            return math.sqrt(q.value.w * q.value.w + q.value.x * q.value.x + q.value.y * q.value.y +
                             q.value.z * q.value.z);
        }
        
        //Auxiliar method for angular velocity computing
        public static float3 AngularVelocity(quaternion current, quaternion next, float deltaTime)
        {
            return ToScaledAngleAxis(Abs(math.mul(next, math.inverse(current)))) / deltaTime;
        }

        public static float3 TranslateToLocal(float3 position, quaternion rotation, float3 globalPosition)
        {
            return math.transform(CreateInverseModel(position, rotation), globalPosition);
        }
        
        public static float3 TranslateToLocal(float4x4 modelInverse, float3 globalPosition)
        {
            return math.transform(modelInverse, globalPosition);
        }

        public static float4x4 CreateInverseModel(float3 position, quaternion rotation)
        {
            return math.inverse(CreateModel(position, rotation));
        }
        
        public static float4x4 CreateModel(float3 position, quaternion rotation)
        {
            return float4x4.TRS(position, rotation, new float3(1));
        }

        public static float3 TranslateToGlobal(float3 position, quaternion rotation, float3 localPosition)
        {
            var model4X4 = float4x4.TRS(position, rotation, new float3(1));
            return math.transform(model4X4, localPosition);
        }

        public static float3 TransformDirection(float4x4 model, float3 localDirection)
        {
            return math.normalize(math.mul(model, new float4(localDirection, 0.0f)).xyz);
        }
        
        public static float3 TransformDirection(float3 position, quaternion rotation, float3 localDirection)
        {
            return TransformDirection(CreateModel(position, rotation), localDirection);
        }

        public static float3 InverseTransformDirection(float4x4 inverseModel, float3 globalDirection)
        {
            return TransformDirection(inverseModel, globalDirection);
        }
        
        public static float3 InverseTransformDirection(float3 position, quaternion rotation, float3 globalDirection)
        {
            return TransformDirection(CreateInverseModel(position, rotation), globalDirection);
        }
        
        public static quaternion DirectionToQuaternion(float3 direction)
        {
            // Usa el método LookRotationSafe para obtener el quaternion que apunta hacia la dirección.
            return quaternion.LookRotationSafe(direction, Float3Ex.Up);
        }
        
        public static float NormalizeHelper(float component, float meanComponent, float stdComponent)
        {
            if (stdComponent == 0)
            {
                return 0;
            }
            return (component - meanComponent) / stdComponent;
        }

        public static float SignedAngle(float3 from, float3 to, float3 axis)
        {
            float num1 = Angle(from, to);
            float num2 = (float) ((double) from.y * (double) to.z - (double) from.z * (double) to.y);
            float num3 = (float) ((double) from.z * (double) to.x - (double) from.x * (double) to.z);
            float num4 = (float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x);
            float num5 = math.sign((float) ((double) axis.x * (double) num2 + (double) axis.y * (double) num3 + (double) axis.z * (double) num4));
            return num1 * num5;
        }

        public static float Angle(float3 from, float3 to)
        {
            float num = (float) math.sqrt((double) math.lengthsq(from) * (double) math.lengthsq(to));
            return (double) num < 1.00000000362749E-15 ? 0.0f : (float) math.acos((double) math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
    }
}
