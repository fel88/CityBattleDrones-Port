using System.Data;

namespace CityBattleDrones_Port
{
    public class Spline
    {
        public Spline()
        {
            // set default boundary condition to be zero curvature at both ends
            m_left = bd_type.second_deriv;
            m_right = bd_type.second_deriv;
            m_left_value = (0.0);
            m_right_value = (0.0);
            m_force_linear_extrapolation = (false);
        }

        enum bd_type
        {
            first_deriv = 1,
            second_deriv = 2
        }
        List<double> m_x, m_y;            // x,y coordinates of points
                                          // interpolation parameters
                                          // f(x) = a*(x-x_i)^3 + b*(x-x_i)^2 + c*(x-x_i) + y_i
        double[] m_a, m_b, m_c;        // spline coefficients
        double m_b0, m_c0;                     // for left extrapol
        bd_type m_left, m_right;
        double m_left_value, m_right_value;
        bool m_force_linear_extrapolation;


        public double At(double x)
        {
            double interpol = 0;
            int n = m_x.Count;
            // find the closest point m_x[idx] < x, idx=0 even if x<m_x[0]

            double it = 0;
            int midx = 0;
            for (int i = 0; i < m_x.Count; i++)
            {
                if (m_x[i] > x)
                {
                    it = m_x[i];
                    midx = i;
                    break;
                }
            }
            //it = std::lower_bound(m_x.begin(), m_x.end(), x);
            int idx = Math.Max(midx - 1, 0);

            double h = x - m_x[idx];

            if (x < m_x[0])
            {
                // extrapolation to the left
                interpol = (m_b0 * h + m_c0) * h + m_y[0];
            }
            else if (x > m_x[n - 1])
            {
                // extrapolation to the right
                interpol = (m_b[n - 1] * h + m_c[n - 1]) * h + m_y[n - 1];
            }
            else
            {
                // interpolation
                interpol = ((m_a[idx] * h + m_b[idx]) * h + m_c[idx]) * h + m_y[idx];
            }
            return interpol;
        }

        internal void set_points(List<double> x, List<double> y, bool cubic_spline = true)
        {
            m_x = x;
            m_y = y;
            int n = x.Count;
            if (cubic_spline)
            {
                // cubic spline interpolation
                // setting up the matrix and right hand side of the equation system
                // for the parameters b[]
                band_matrix A = new band_matrix(n, 1, 1);
                double[] rhs = new double[(n)];
                for (int i = 1; i < n - 1; i++)
                {
                    A.Set(i, i - 1, 1.0 / 3.0 * (x[i] - x[i - 1]));
                    A.Set(i, i, 2.0 / 3.0 * (x[i + 1] - x[i - 1]));
                    A.Set(i, i + 1, 1.0 / 3.0 * (x[i + 1] - x[i]));
                    rhs[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
                }

                // solve the equation system to obtain the parameters b[]
                m_b = A.lu_solve(rhs);

                // calculate parameters a[] and c[] based on b[]
                m_a = new double[n];
                m_c = new double[(n)];
                for (int i = 0; i < n - 1; i++)
                {
                    m_a[i] = 1.0 / 3.0 * (m_b[i + 1] - m_b[i]) / (x[i + 1] - x[i]);
                    m_c[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i])
                    - 1.0 / 3.0 * (2.0 * m_b[i] + m_b[i + 1]) * (x[i + 1] - x[i]);
                }
            }
            else
            { // linear interpolation
                m_a = new double[n];
                m_b = new double[n];
                m_c = new double[n];
                for (int i = 0; i < n - 1; i++)
                {
                    m_a[i] = 0.0;
                    m_b[i] = 0.0;
                    m_c[i] = (m_y[i + 1] - m_y[i]) / (m_x[i + 1] - m_x[i]);
                }
            }

            // for left extrapolation coefficients
            m_b0 = (m_force_linear_extrapolation == false) ? m_b[0] : 0.0;
            m_c0 = m_c[0];

            // for the right extrapolation coefficients
            // f_{n-1}(x) = b*(x-x_{n-1})^2 + c*(x-x_{n-1}) + y_{n-1}
            double h = x[n - 1] - x[n - 2];
            // m_b[n-1] is determined by the boundary condition
            m_a[n - 1] = 0.0;
            m_c[n - 1] = 3.0 * m_a[n - 2] * h * h + 2.0 * m_b[n - 2] * h + m_c[n - 2];   // = f'_{n-2}(x_{n-1})
            if (m_force_linear_extrapolation == true)
                m_b[n - 1] = 0.0;

        }
    }
}
