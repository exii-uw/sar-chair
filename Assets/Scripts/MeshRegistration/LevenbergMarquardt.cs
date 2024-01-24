using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LevenbergMarquardt
{
    // y_i - f(x_i, parameters) as column vector
    public delegate Matrix Function(Matrix parameters);
    public delegate void ParameterProgressUpdate(Matrix parameters);

    public LevenbergMarquardt(Function function, ParameterProgressUpdate _parameterProgress)
        : this(function, _parameterProgress, new NumericalDifferentiation(function).Jacobian)
    {
    }

    // J_ij, ith error from function, jth parameter
    public delegate Matrix Jacobian(Matrix parameters);

    public LevenbergMarquardt(Function function, ParameterProgressUpdate _parameterProgress, Jacobian jacobianFunction)
    {
        this.function = function;
        this.jacobianFunction = jacobianFunction;
        this.parameterProgress = _parameterProgress;
    }

    public enum States { Running, MaximumIterations, LambdaTooLarge, ReductionStepTooSmall, MinimumErrorReached };
    public double RMSError { get { return rmsError; } }
    public States State { get { return state; } }

    public int maximumIterations = 100;
    public double minimumReduction = 1.0e-5;
    public double maximumLambda = 1.0e9;
    public double lambdaIncrement = 10.0;
    public double initialLambda = 1.0e-3;
    public double minimumErrorTolerance = 1.0e-3;


    Function function;
    Jacobian jacobianFunction;
    ParameterProgressUpdate parameterProgress;
    States state = States.Running;
    double rmsError;

    public double Minimize(Matrix parameters)
    {
        state = States.Running;
        for (int iteration = 0; iteration < maximumIterations; iteration++)
        {
            MinimizeOneStep(parameters);
            if (parameterProgress != null)
            {
                Matrix tmp = new Matrix(parameters.Size, 1);
                tmp.Copy(parameters);
                parameterProgress(tmp);
            }
            if (state != States.Running)
                return RMSError;
        }
        state = States.MaximumIterations;
        return RMSError;
    }

    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    public double MinimizeOneStep(Matrix parameters)
    {
        // initial value of the function; callee knows the size of the returned vector
        var errorVector = function(parameters);
        var error = errorVector.Dot(errorVector);

        // Jacobian; callee knows the size of the returned matrix
        var J = jacobianFunction(parameters);

        // J'*J
        var JtJ = new Matrix(parameters.Size, parameters.Size);
        //stopWatch.Restart();
        //JtJ.MultATA(J, J); // this is the big calculation that could be parallelized
        JtJ.MultATAParallel(J, J);
        //Console.WriteLine("JtJ: J size {0}x{1} {2}ms", J.Rows, J.Cols, stopWatch.ElapsedMilliseconds);

        // J'*error
        var JtError = new Matrix(parameters.Size, 1);
        //stopWatch.Restart();
        JtError.MultATA(J, errorVector); // error vector must be a column vector
                                         //Console.WriteLine("JtError: errorVector size {0}x{1} {2}ms", errorVector.Rows, errorVector.Cols, stopWatch.ElapsedMilliseconds);



        // allocate some space
        var JtJaugmented = new Matrix(parameters.Size, parameters.Size);
        var JtJinv = new Matrix(parameters.Size, parameters.Size);
        var delta = new Matrix(parameters.Size, 1);
        var newParameters = new Matrix(parameters.Size, 1);

        // find a value of lambda that reduces error
        double lambda = initialLambda;
        int iteration = 0;
        while (true)
        {
            // augment J'*J: J'*J += lambda*(diag(J))
            JtJaugmented.Copy(JtJ);
            for (int i = 0; i < parameters.Size; i++)
                JtJaugmented[i, i] = (1.0 + lambda) * JtJ[i, i];

            //WriteMatrixToFile(errorVector, "errorVector");
            //WriteMatrixToFile(J, "J");
            //WriteMatrixToFile(JtJaugmented, "JtJaugmented");
            //WriteMatrixToFile(JtError, "JtError");


            // solve for delta: (J'*J + lambda*(diag(J)))*delta = J'*error
            JtJinv.Inverse(JtJaugmented);
            delta.Mult(JtJinv, JtError);

            // new parameters = parameters - delta [why not add?]
            newParameters.Sub(parameters, delta);
                
            // evaluate function, compute error
            var newErrorVector = function(newParameters);
            double newError = newErrorVector.Dot(newErrorVector);

            // if error is reduced, divide lambda by 10
            bool improvement;
            if (newError < error)
            {
                Console.WriteLine("Imporvement Made : " + newError.ToString() + "   " + error.ToString());
                lambda /= lambdaIncrement;
                improvement = true;
            }
            else // if not, multiply lambda by 10
            {
                lambda *= lambdaIncrement;
                improvement = false;
            }
            Console.WriteLine("Iteraction " + iteration.ToString() + ": " + error.ToString());

            // termination criteria:
            // reduction in error is too small
            var diff = new Matrix(errorVector.Size, 1);
            diff.Sub(errorVector, newErrorVector);
            double diffSq = diff.Dot(diff);
            double errorDelta = Math.Sqrt(diffSq / error);
            double rms = Math.Sqrt(error / errorVector.Size);
            
            if (rms < minimumErrorTolerance)
            {
                state = States.MinimumErrorReached;
            }

            if (errorDelta < minimumReduction)
                state = States.ReductionStepTooSmall;

            // lambda is too big
            if (lambda > maximumLambda)
                state = States.LambdaTooLarge;

            // change in parameters is too small [not implemented]

            // if we made an improvement, accept the new parameters
            if (improvement)
            {
                parameters.Copy(newParameters);
                error = newError;
                break;
            }

            // if we meet termination criteria, break
            if (state != States.Running)
                break;
        }

        rmsError = Math.Sqrt(error / errorVector.Size);
        return rmsError;
    }





    public class NumericalDifferentiation
    {
        public NumericalDifferentiation(Function function)
        {
            this.function = function;
        }

        // J_ij, ith error from function, jth parameter
        public Matrix Jacobian(Matrix parameters)
        {
            //const double deltaFactor = 1.0e-6;
            //const double minDelta = 1.0e-6;

            const double deltaFactor = 1.0e-4;
            const double minDelta = 1.0e-4;


            //const double deltaFactor = 0.001;
            //const double minDelta = 0.001;

            // evaluate the function at the current solution
            var errorVector0 = function(parameters);
            var J = new Matrix(errorVector0.Size, parameters.Size);

            // vary each paremeter
            for (int j = 0; j < parameters.Size; j++)
            {
                double parameterValue = parameters[j]; // save the original value

                double delta = parameterValue * deltaFactor;
                if (Math.Abs(delta) < minDelta)
                {
                    double sign = Math.Sign(parameterValue);
                    sign = sign == 0 ? 1 : sign;
                    delta = sign * minDelta;
                }

                parameters[j] = parameters[j] + delta;

                // we only get error from function, but error(p + d) - error(p) = f(p + d) - f(p)
                var errorVector = function(parameters);
                errorVector.Sub(errorVector0);

                for (int i = 0; i < errorVector0.Rows; i++)
                    J[i, j] = errorVector[i] / delta;
                parameters[j] = parameterValue; // restore original value
            }
            return J;
        }

        Function function;
    }

    static public void Test()
    {
        // generate x_i, y_i observations on test function

        var random = new Random();

        int n = 200;

        var X = new Matrix(n, 1);
        var Y = new Matrix(n, 1);

        {
            double a = 100; double b = 102;
            for (int i = 0; i < n; i++)
            {
                double x = random.NextDouble() / (Math.PI / 4.0) - Math.PI / 8.0;
                double y = a * Math.Cos(b * x) + b * Math.Sin(a * x) + random.NextDouble() * 0.1;
                X[i] = x;
                Y[i] = y;
            }
        }


        Function f = delegate (Matrix parameters)
        {
            // return y_i - f(x_i, parameters) as column vector
            var error = new Matrix(n, 1);

            double a = parameters[0];
            double b = parameters[1];

            for (int i = 0; i < n; i++)
            {
                double y = a * Math.Cos(b * X[i]) + b * Math.Sin(a * X[i]);
                error[i] = Y[i] - y;
            }

            return error;
        };


        var levenbergMarquardt = new LevenbergMarquardt(f, null);

        var parameters0 = new Matrix(2, 1);
        parameters0[0] = 90;
        parameters0[1] = 96;

        var rmsError = levenbergMarquardt.Minimize(parameters0);


    }

}