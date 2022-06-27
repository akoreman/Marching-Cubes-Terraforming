void Float_to_color_gradient_float(float input, out float3 output)
{
    output = input  * float3(1,0,1) + (10 - input) * (1,0,0);
}