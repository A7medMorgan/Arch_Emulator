using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Archi_Emulator
{
    public partial class Form1 : Form
    {
        string instructions;
        // constant
        public const string op_spliter = " ";
        public const char _spliter = ':';
        public const string operation = "#";
        public const string r_type = "000000";
        public const string i_type = "101011";

        // Program Counter
        public string PC;  // reg value
        public int Cycle_Nu;

        // Pipeline Register
                                    // for sequntial excute
            // read only reg  
        public string r_IF_ID = null;
        public string r_ID_EX = null;
        public string r_EX_MEM = null;
        public string r_MEM_WB = null;
            // write only reg
        public string w_IF_ID = null;
        public string w_ID_EX = null;
        public string w_EX_MEM = null;
        public string w_MEM_WB = null;
        
        // Register
        public string[] register_file;
        public const int reg_count = 32;

        //Memory
        int mem_size = 1024;
        public Hashtable mem_array;
        
        //Instruction
        public Hashtable mem_intruction;

        // Clock
        public static Queue<string> _Fetch = new Queue<string>();
        public static Queue<string> _Decode = new Queue<string>();
        public static Queue<string> _Excude = new Queue<string>();
        public static Queue<string> _Memory = new Queue<string>();
        public static Queue<string> _Write_back = new Queue<string>();


        public Form1()
        {
            InitializeComponent();
        }
        // intial
        void intialize()
        {
            intialze_reg_file_list();  // set register array value
            intialize_mem_array();   // set memory array value
            get_instruction();  // set instruction table
            Hundel_clock();  // to make the gape between the cycles
            PC = textBox1.Text;   // set PC Intial value
            Cycle_Nu = 0;
        }
        void Hundel_clock()
        {
            _Fetch.Enqueue(PC);  // refer to operation

            // Decode gape
            _Decode.Enqueue(operation);
            
            //Exute Gape
            _Excude.Enqueue(operation);
            _Excude.Enqueue(operation);
           
            //Memory Gape
            _Memory.Enqueue(operation);
            _Memory.Enqueue(operation);
            _Memory.Enqueue(operation);
            
            // Write back gape
            _Write_back.Enqueue(operation);
            _Write_back.Enqueue(operation);
            _Write_back.Enqueue(operation);
            _Write_back.Enqueue(operation);
        }
        public void intialze_reg_file_list()
        {
            register_file = new string[reg_count];
            register_file[0] = "0";
            for (int i = 1; i < reg_count; i++)
                register_file[i] = (i + 100).ToString();
            write_reg_grid_view();
        }
        public void write_reg_grid_view()
        {
            dataGridView1.Rows.Clear();
            string value;
            for (int i = 1; i < reg_count; i++)
            {
                value = register_file[i];
                object[] rowdata = { "$" + i.ToString(), value };
                dataGridView1.Rows.Add(rowdata);
            }
        }
        public void intialize_mem_array()
        {
           
            mem_array = new Hashtable();
            for (int i = 0; i < mem_size; i++)
                mem_array.Add ((100+(i*4)).ToString(),"99");
            write_mem_grid_view();
        }
        public void write_mem_grid_view()
        {
            dataGridView2.Rows.Clear();
            for (int i = 0; i < mem_size; i++)
            {
                int key = (100 + (i * 4));
                object[] rowdata = { key.ToString(), (string)mem_array[key.ToString()] };
                dataGridView2.Rows.Add(rowdata);
            }
        }

        public void write_pipeline_grid_view()
        {
            dataGridView3.Rows.Clear();
            object[] rowdata1 = { "IF/ID REGISTER", w_IF_ID };
            dataGridView3.Rows.Add(rowdata1);
            object[] rowdata2 = { "ID/EX REGISTER", w_ID_EX };
            dataGridView3.Rows.Add(rowdata2);
            object[] rowdata3 = { "EX/MEM REGISTER", w_EX_MEM };
            dataGridView3.Rows.Add(rowdata3);
            object[] rowdata4 = { "MEM/WB REGISTER", w_MEM_WB };
            dataGridView3.Rows.Add(rowdata4);
        }
        void get_instruction()
        {
            if (UserInput.Text.Length == 0)
                return;
            mem_intruction = new Hashtable();
            instructions = UserInput.Text;
            // seprete lines
            string[] lines = instructions.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] _instruction = lines[i].Split(_spliter);
                string pc = _instruction[0];
                string op_instraction = _instruction[1];
                mem_intruction.Add(pc, op_instraction);
            }
        }

        // control unit

        public string control_unit(string op_code)
        {
            string excu = null, mem = null, wb=null;
            string reg_dst, aluop1, aluop0, alusrc, bruch, mem_read, mem_write, reg_write, mem_to_reg; 
            if (op_code.Equals(r_type)) // r_type
            {
                reg_dst = "1";
                aluop1 = "1";
                aluop0 = "0";
                alusrc = "0";
                excu = reg_dst + aluop1 + aluop0 + alusrc;
                bruch = "0";
                mem_read = "0";
                mem_write = "0";
                mem = bruch + mem_read + mem_write;
                reg_write = "1";
                mem_to_reg = "0";
                wb = reg_write + mem_to_reg;
            }
            else if (op_code.Equals(i_type)) // i_type sw
            {
                reg_dst = "-";
                aluop1 = "0";
                aluop0 = "0";
                alusrc = "1";
                excu = reg_dst + aluop1 + aluop0 + alusrc;
                bruch = "0";
                mem_read = "0";
                mem_write = "1";
                mem = bruch + mem_read + mem_write;
                reg_write = "0";
                mem_to_reg = "-";
                wb = reg_write + mem_to_reg;
            }
            else
            {
                // not avalid instruction
            }
            return excu + mem + wb;
        }

        // register file hundeler

        int reg_file(int read_reg_1, int read_reg_2, int write_reg, string write_date, int flag_reg_write, ref string reg1_read, ref string reg2_read)
        {
            if (flag_reg_write == 0)
            {
                reg1_read = register_file[read_reg_1];
                reg2_read = register_file[read_reg_2];
                return 1;
            }
            else if (flag_reg_write == 1)
            {
                register_file[write_reg] = write_date;
            }
            return 0;
        }

        // memory hundeler

        int mem_file(string address, string write_data, int flag_write_mem, int flag_read_mem,ref string data)
        {
            if (flag_read_mem == 1)
            {
                data = (string)mem_array[address];
                return 1;
            }
            else if (flag_write_mem == 1)
            {
                mem_array.Remove(address);
                mem_array.Add(address, write_data);
                return 1;
            }
            else
            {
                // faild to access the memory
            }
            return 0;
        }

        // ALU hundeler 
        string ALU(string data1, string data2, string alu_op, string func_code)
        {
            int result;
            if (alu_op.Equals("00"))  // i type sw | lw
            {
                result = Convert.ToInt32(data1,2) + Convert.ToInt32(data2,2);
                return result.ToString();
            }
            else if (alu_op.Equals("10"))  // r type
            {
                if (func_code.Equals("100000")) // add
                {
                    result = Int32.Parse(data1) + Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100010")) // subtract
                {
                    result = Int32.Parse(data1) - Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100100")) // AND
                {
                    result = Int32.Parse(data1) & Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100101"))  // OR
                {
                    result = Int32.Parse(data1) | Int32.Parse(data2);
                    return result.ToString();
                }
                else
                {
                    // not valid
                }
            }
            return null;
        }

        // multiplexer hundeler
        string MUX(string input1, string input2, char selector)
        {
            if (selector.Equals('0'))
            {
                return input1;
            }
            else
            {
                return input2;
            }
        }

        // Sign Extended hundeler
        public string Sign_Extend(string offset)
        {
            if (offset[0].Equals('0'))
            {
                return "0000000000000000".ToString() + offset;
            }
            else if (offset[0].Equals('1'))
            {
                return "1111111111111111".ToString() + offset;
            }
            return null;
        }

        // get instruction format
        string OP_format(string instruction,ref string type)
        {
            string op_code = instruction.Substring(0, 6);

            if (op_code.Equals(r_type))
            {
                string rs = instruction.Substring(6, 5);
                string rt = instruction.Substring(11, 5);
                string rd = instruction.Substring(16, 5);
                string shamt = instruction.Substring(21, 5);
                string funct = instruction.Substring(26, 6);
                type = r_type;
                return op_code + op_spliter + rs + op_spliter + rt + op_spliter + rd + op_spliter + shamt + op_spliter + funct;
            }
            else if (op_code.Equals(i_type))
            {
                string rs = instruction.Substring(6, 5);
                string rt = instruction.Substring(11, 5);
                string addres = instruction.Substring(16, 16);
                type = i_type;
                return op_code + op_spliter + rs + op_spliter + rt + op_spliter + addres;
            }
            else
            {
                type = null;
            }
            return null;
        }
        //                               ###################### Fetch #################################
        // Fetch state
        public void Fetch()
        {
            string _PC = null;
            
            if (_Fetch.Count != 0)
            {
                _PC = _Fetch.Dequeue();
            }


            string instruction = null;
            if (mem_intruction.ContainsKey(PC))
            {
                instruction = (string)mem_intruction[PC];

                //PC += 4;
                int _pc = Convert.ToInt32(PC) + 4;
                PC = Convert.ToString(_pc);

                _Fetch.Enqueue(PC);

                w_IF_ID = PC + _spliter + instruction;

                _Decode.Enqueue(w_IF_ID);
            }
            else
            {
                PC = "0000";
                // End of PC sequance
            }
            

            //string instruction = (string)mem_intruction[PC];
            //r_IF_ID = w_IF_ID;
            //w_IF_ID = PC + _spliter + instruction;

            //if (instruction != null)
            //{
            //    int _pc = Convert.ToInt32(PC) + 4;
            //    PC = Convert.ToString(_pc);
            //}
            textBox1.Text = PC;
        }

        //                              ##################### Decode ################################
        // Decode Statg

        public void Decode()
        {
            if (_Decode.Count != 0)
            {
               r_IF_ID = _Decode.Dequeue();
                if (r_IF_ID == operation)
                    return;
            }
            else
                return;

            string read_reg1 = null, read_reg2= null, address_extented = null, rt = null, rd=null;

            string[] if_id = r_IF_ID.Split(_spliter);  //get the register PL value


            string type=null;
            string[] op_parts = (OP_format(if_id[1], ref type)).Split(op_spliter[0]); // seprate the operation part

            string control_bits = control_unit(op_parts[0]); // activate the control unit and get the instructions bits

            int rs = Convert.ToInt32(op_parts[1],2);  // rs       // convert string bits to int  integer address acces
            int _rt =  Convert.ToInt32(op_parts[2],2); // rt          // convert string bits to int

            //int reg_write = Int32.Parse(control_bits[7].ToString()); // get reg_write bit

            if (type.Equals(r_type))
            {
                int i = reg_file(rs, _rt, 0, null, 0, ref read_reg1, ref read_reg2);
                rt = op_parts[2];  // get rt to pass
                rd = op_parts[3];  // get rd  to pass 
                address_extented = Sign_Extend(op_parts[3] + op_parts[4] + op_parts[5]); // rd(5) + shamt(5) + fcode(6) bits

            }
            else if (type.Equals(i_type)) // sw
            {
                //int i = reg_file(rs, _rt, 0, null, reg_write, ref read_reg1, ref read_reg2);
                int i = reg_file(rs, _rt, 0, null, 0, ref read_reg1, ref read_reg2);
                rt = op_parts[2];
                rd = op_parts[3].Substring(0,5);  // get the rd from address
                address_extented = Sign_Extend(op_parts[3]); 

            }
            w_ID_EX =if_id[0] + op_spliter + control_bits + op_spliter + read_reg1 + op_spliter + read_reg2 + op_spliter + address_extented + op_spliter + rt + op_spliter + rd;
            _Excude.Enqueue(w_ID_EX);
        }

        //                 ####################### Excude ##########################
        // excude fun
        public void Excude()
        {
            if (_Excude.Count != 0)
            {
                r_ID_EX = _Excude.Dequeue();
                if (r_ID_EX == operation)
                    return;
            }
            else
                return;

            string[] id_ex = r_ID_EX.Split(op_spliter[0]);

            string control_bits = id_ex[1];
            string alu_op = control_bits[1] +""+ control_bits[2]; // get access for the alu operation bits

            string data1 = id_ex[2];
            string data2 = MUX(id_ex[3],id_ex[4],control_bits[3]);  // mux of data two input       mux alu_src

            string fun_code = id_ex[4].Substring(26,6);// fun code 6 bits from the address extended

            string result = ALU(data1, data2, alu_op, fun_code);

            w_EX_MEM =id_ex[0]+ op_spliter +"0" + op_spliter + result + op_spliter + id_ex[3] + op_spliter + control_bits + op_spliter + MUX(id_ex[5], id_ex[6], control_bits[0]);  // mux reg_dst
            _Memory.Enqueue(w_EX_MEM);
        }

        //                ########################### MEMORY #######################
        // mem access
        public void Memory()
        {
            if (_Memory.Count != 0)
            {
               r_EX_MEM = _Memory.Dequeue();
                if (r_EX_MEM == operation)
                    return;
            }
            else
                return;

            string[] ex_mem = r_EX_MEM.Split(op_spliter[0]);
            string control_bits = ex_mem[4];
            string reg_dst = ex_mem[5];

            string alu_result = ex_mem[2];
            //string address = (Convert.ToInt32(alu_result, 2)).ToString();
            string address = alu_result;

             string data = "-";
            int result = mem_file(address, ex_mem[3],Convert.ToInt32(control_bits[6]),Convert.ToInt32(control_bits[5]),ref data);

            w_MEM_WB = data + op_spliter + alu_result + op_spliter + reg_dst + op_spliter + control_bits;

            _Write_back.Enqueue(w_MEM_WB);
        }

        //            ########################## WRITE BACK ########################
        // write back to reg
        public void Write_back()
        {
            if (_Write_back.Count != 0)
            {
                r_MEM_WB = _Write_back.Dequeue();
                if (r_MEM_WB == operation)
                    return;
            }
            else
                return;

            string[] mem_wb = r_MEM_WB.Split(op_spliter[0]);
            string control_bits = mem_wb[3];

            string write_data = MUX( mem_wb[1], mem_wb[0], control_bits[8]); // mux  mem to reg
                                //  alu_result(address) ,   read_data ,   mem_to_reg

            string n1 = null, n2 = null;

            string _reg_dst = mem_wb[2];
            int reg_dst = Convert.ToInt32(_reg_dst, 2);

             reg_file(0, 0, reg_dst, write_data,Int32.Parse(control_bits[7].ToString()),ref n1,ref n2);
        }

        public void Cycle()
        {
            Fetch();
            Decode();
            Excude();
            Memory();
            Write_back();

            Cycle_Nu++;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            intialize();

            //string p = "01011";
            //string y = "01001";
            //Console.WriteLine(Convert.ToInt32(p,2)+1 + "&" + int.Parse(p)+1 + "&" + Int32.Parse(p)+Int32.Parse(y));
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            Cycle();

            write_pipeline_grid_view();
            write_reg_grid_view();
            write_mem_grid_view();

            button2.Text = "Cycle ::" + Cycle_Nu.ToString();
        }
    }
}
