using System.Drawing;

namespace CityBattleDrones_Port
{
    public class band_matrix
    {
        public band_matrix(int dim, int n_u, int n_l)
        {
            resize(dim, n_u, n_l);
        }
        void resize(int dim, int n_u, int n_l)
        {
            //assert(dim > 0);
            //assert(n_u >= 0);
            //assert(n_l >= 0);
            m_upper = new double[(n_u + 1)][];
            m_lower = new double[(n_l + 1)][];
            for (int i = 0; i < m_upper.Count(); i++)
            {
                m_upper[i] = new double[(dim)];
            }
            for (int i = 0; i < m_lower.Count(); i++)
            {
                m_lower[i] = new double[(dim)];
            }
        }
        double[][] m_upper;  // upper band
        double[][] m_lower;  // lower band
        public double[] lu_solve(double[] b, bool is_lu_decomposed = false)
        {
            //assert(this->dim() == (int)b.size());
            double[] x, y;
            if (is_lu_decomposed == false)
            {
                lu_decompose();
            }
            y = l_solve(b);
            x = r_solve(y);
            return x;
        }
        // solves Rx=y
        double[] r_solve(double[] b)
        {
            //assert(this->dim() == (int)b.size());
            double[] x = new double[(dim())];
            int j_stop;
            double sum;
            for (int i = dim() - 1; i >= 0; i--)
            {
                sum = 0;
                j_stop = Math.Min(dim() - 1, i + num_upper());
                for (int j = i + 1; j <= j_stop; j++) sum += Get(i, j) * x[j];
                x[i] = (b[i] - sum) / Get(i, i);
            }
            return x;
        }
        // solves Ly=b
        double[] l_solve(double[] b)
        {
            //assert(this->dim() == (int)b.size());
            double[] x = new double[dim()];
            int j_start;
            double sum;
            for (int i = 0; i < dim(); i++)
            {
                sum = 0;
                j_start = Math.Max(0, i - num_lower());
                for (int j = j_start; j < i; j++) sum += Get(i, j) * x[j];
                x[i] = (b[i] * saved_diag(i)) - sum;
            }
            return x;
        }

        private double Get(int i, int j)
        {
            int k = j - i;       // what band is the entry
            //assert((i >= 0) && (i < dim()) && (j >= 0) && (j < dim()));
            //assert((-num_lower() <= k) && (k <= num_upper()));
            // k=0 -> diogonal, k<0 lower left part, k>0 upper right part
            if (k >= 0) return m_upper[k][i];
            else return m_lower[-k][i];
        }
        // second diag (used in LU decomposition), saved in m_lower
        double saved_diag(int i)
        {
            //assert((i>=0) && (i<dim()) );
            return m_lower[0][i];
        }
        void set_saved_diag(int i, double val)
        {
            //assert((i >= 0) && (i < dim()));
            m_lower[0][i] = val;
        }
        int num_upper()
        {
            return m_upper.Count() - 1;
        }
        int num_lower()
        {
            return m_lower.Count() - 1;
        }
        int dim()
        {
            if (m_upper.Count() > 0)
            {
                return m_upper[0].Count();
            }
            else
            {
                return 0;
            }
        }

        // LR-Decomposition of a band matrix
        void lu_decompose()
        {

            int i_max, j_max;
            int j_min;
            double x;


            // preconditioning
            // normalize column i so that a_ii=1
            for (int i = 0; i < dim(); i++)
            {
                //assert(this->operator()(i, i) != 0.0);
                set_saved_diag(i, 1.0 / Get(i, i));
                j_min = Math.Max(0, i - num_lower());
                j_max = Math.Min(dim() - 1, i + num_upper());
                for (int j = j_min; j <= j_max; j++)
                {
                    Set(i, j, Get(i, j) * saved_diag(i));
                }
                Set(i, i, 1.0);          // prevents rounding errors
            }

            // Gauss LR-Decomposition
            for (int k = 0; k < dim(); k++)
            {
                i_max = Math.Min(dim() - 1, k + num_lower());  // num_lower not a mistake!
                for (int i = k + 1; i <= i_max; i++)
                {
                    //assert(this->operator()(k, k) != 0.0);
                    x = -Get(i, k) / Get(k, k);
                    Set(i, k, -x);                         // assembly part of L
                    j_max = Math.Min(dim() - 1, k + num_upper());
                    for (int j = k + 1; j <= j_max; j++)
                    {
                        // assembly part of R
                        Set(i, j, Get(i, j) + x * Get(k, j));
                    }
                }
            }
        }

        internal void Set(int i, int j, double v)
        {
            int k = j - i;       // what band is the entry
            //assert((i >= 0) && (i < dim()) && (j >= 0) && (j < dim()));
            //assert((-num_lower() <= k) && (k <= num_upper()));
            // k=0 -> diogonal, k<0 lower left part, k>0 upper right part
            if (k >= 0) m_upper[k][i] = v;
            else m_lower[-k][i] = v;
        }
    }
}
